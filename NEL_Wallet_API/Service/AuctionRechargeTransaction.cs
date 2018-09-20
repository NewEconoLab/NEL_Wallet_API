using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace NEL_Wallet_API.Service
{
    public class AuctionRechargeTransaction
    {
        public mongoHelper mh { set; get; }
        public string notify_mongodbConnStr { get; set; }
        public string notify_mongodbDatabase { get; set; }
        public string cgasMergeTxCol { get; set; }
        public int batchSendInterval { get; set; } = 3000;
        public string neoCliJsonRPCUrl { get; set; }
        public string netType { get; set; } = "testnet";

        public void sendTxLoop()
        {
            int cnt = 0;
            while(true)
            {
                if(++cnt > 20)
                {
                    heartBeat(); cnt = 0;
                }
                Thread.Sleep(batchSendInterval);
                try
                {
                    process();
                } catch(Exception ex)
                {
                    printEx(ex);
                }
            }
        }

        private void process()
        {
            string findstr = new JObject() { { "state", "" }, { "txid2txhex", new JObject(){{"$ne",null } } } }.ToString();
            string fieldstr = new JObject() { { "txid2", 1 }, { "txid2txhex", 1 }, { "txid1", 1 } }.ToString();
            JArray res = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, cgasMergeTxCol, fieldstr, findstr);
            if (res == null || res.Count == 0) return;

            res.Select(p =>
            {
                // 过滤历史交易记录
                if (p["txid2txhex"].ToString() == "") return "";
                
                // 检查第一笔是否入链
                string txid1 = p["txid1"].ToString();
                if (!getTx(txid1)) return "";

                // 检查第二笔交易是否发送
                TxStateCode txState = null;
                string errMsg = "";
                string txid2 = p["txid2"].ToString();
                if(txid2 == "")
                {
                    string txHex = p["txid2txhex"].ToString();
                    txid2 = getTxid(txHex);
                    bool result = false;
                    try
                    {
                        result = sendTx(txHex, out errMsg);
                    }
                    catch (Exception ex)
                    {
                        printEx(ex, txid2);
                    }
                    if(!result)
                    {
                        txState = TxState.TX_INTERRUPT;
                    }
                }
                // 检查第二笔是否成功入链
                string state = "";
                if(getTx(txid2))
                {
                    txState = TxState.TX_SECC;
                    state = "1"; // 结束处理
                } else
                {
                    // 超过多次，则置为失败状态...

                }
                string tfindstr = new JObject() { { "txid1", txid1 } }.ToString();
                string tnewdata = new JObject() { { "$set", new JObject() { { "txid2", txid2 }, { "txid2Code", txState.code }, { "txid2CodeMessage", txState.codeMessage }, { "txid2errMsg", errMsg }, { "lastUpdateTime", TimeHelper.GetTimeStamp() },{"state",state } } } }.ToString();
                mh.UpdateData(notify_mongodbConnStr, notify_mongodbDatabase, cgasMergeTxCol, tnewdata, tfindstr);
                return "";
            }).ToArray();
        }

        public bool getTx(string txid)
        {
            return getTx(neoCliJsonRPCUrl, txid);
        }
        public static bool getTx(string neoCliJsonRPCUrl, string txid)
        {
            var resp = httpHelper.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'getrawtransaction','params':['" + txid + "'],'id':1}", System.Text.Encoding.UTF8, 1);
            JObject res = JObject.Parse(resp);
            if (res["result"] == null)
            {
                return false;
            }
            return true;
        }
        private bool sendTx(string txSigned, out string err)
        {
            return sendSignedTx(neoCliJsonRPCUrl, txSigned, out err);
        }

        public static bool sendSignedTx(string neoCliJsonRPCUrl, string txSigned, out string err)
        {
            var resp = httpHelper.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'sendrawtransaction','params':['" + txSigned + "'],'id':1}", System.Text.Encoding.UTF8, 1);
            JObject res = JObject.Parse(resp);
            if(res["result"] == null)
            {
                err = Convert.ToString(res["error"]);
                return false;
            }
            err = "";
            return (bool)JObject.Parse(resp)["result"];
        }

        private string getTxid(string txSigned)
        {
            return getTxidFromSignedTx(txSigned);
        }
        public static string getTxidFromSignedTx(string txSigned)
        {
            ThinNeo.Transaction lastTran = new ThinNeo.Transaction();
            lastTran.Deserialize(new MemoryStream(txSigned.HexString2Bytes()));
            return lastTran.GetHash().ToString();
        }
        public void printEx(Exception ex, string txid="")
        {
            File.AppendAllText(netType + "_auctionRechargeTx.log", DateTime.Now + " txid=" + txid + ",errMsg="+ex.Message + "\r\n");
        }
        public void heartBeat()
        {
            File.AppendAllText(netType + "_auctionRechargeTx.log", DateTime.Now + " RechargeTxLoop is running" + "\r\n");
        }
    }
}
