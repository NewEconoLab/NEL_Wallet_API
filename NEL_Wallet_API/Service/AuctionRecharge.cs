using NEL_Wallet_API.lib;
using NEL_Wallet_API.Service;
using Newtonsoft.Json.Linq;
using System;

namespace NEL_Wallet_API.Controllers
{
    public class AuctionRecharge
    {
        public string Notify_mongodbConnStr { set; get; }
        public string Notify_mongodbDatabase { set; get; }
        public mongoHelper mh { set; get; }
        public string nelJsonRPCUrl { get; set; }
        public string rechargeCollection { get; set; }

        public JArray rechargeAndTransfer(string txhex1, string txhex2)
        {
            JObject res = null;
            // 发送第一笔交易
            string err = "";
            bool result = AuctionRechargeTransaction.sendSignedTx(nelJsonRPCUrl, txhex1, out err);
            if(!result)
            {
                // 第一笔失败，直接返回
                res = new JObject() {
                    { "errCode", TxState.TX_FAILD.code},
                    { "errMessage", TxState.TX_FAILD.codeMessage},
                    { "txid", ""}
                };
            } else
            {
                // 第二笔成功，入库，返回
                string txid1 = AuctionRechargeTransaction.getTxidFromSignedTx(txhex1);
                saveTxState(txid1, TxState.TX_SECC, "", TxState.TX_WAITING, txhex2);
                res = new JObject() {
                    { "errCode", TxState.TX_SECC.code},
                    { "errMessage", TxState.TX_SECC.codeMessage},
                    { "txid", txid1}
                };
            }
            // 第二笔交由另一线程单独发送...
            return new JArray() { res };
        }

        private void saveTxState(string txid1, TxStateCode txid1Code, string txid2, TxStateCode txid2Code, string txhex2, bool isReplace = false)
        {
            JObject param = new JObject();
            param.Add("txid1", txid1);
            param.Add("txid1Code", txid1Code.code);
            param.Add("txid1CodeMessage", txid1Code.codeMessage);
            param.Add("txid2", txid2);
            param.Add("txid2Code", txid2Code.code);
            param.Add("txid2CodeMessage", txid2Code.codeMessage);
            param.Add("txid2txhex", txhex2);
            param.Add("txid2errMsg", "");
            long time = TimeHelper.GetTimeStamp();
            param.Add("createTime", time);
            param.Add("lastUpdateTime", time);
            param.Add("state", "");

            if (!isReplace)
            {
                mh.InsertOneData(Notify_mongodbConnStr, Notify_mongodbDatabase, rechargeCollection, param.ToString());
            }
            else
            {
                JObject filter = new JObject();
                filter.Add("txid1", txid1);
                mh.ReplaceData(Notify_mongodbConnStr, Notify_mongodbDatabase, rechargeCollection, filter.ToString(), param.ToString());
            }

        }
        
        public JArray getRechargeAndTransfer(string txid)
        {
            JObject res = null;
            //
            string findstr = new JObject() { { "txid1", txid } }.ToString();
            string fieldstr = MongoFieldHelper.toReturn(new string[] { "txid2Code" , "txid2CodeMessage" }).ToString();
            JArray result = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, rechargeCollection, findstr);
            if (result != null && result.Count > 0)
            {
                res = new JObject()
                {
                    {"errCode", Convert.ToString(result[0]["txid2Code"]) },
                    {"errMessage", Convert.ToString(result[0]["txid2CodeMessage"]) },
                    {"txid", "" },
                };
            }
            else
            {
                res = new JObject() {
                    { "errCode", TxState.TX_NOTFIND.code},
                    { "errMessage", TxState.TX_NOTFIND.codeMessage},
                    { "txid", ""}
                };
            }
            return new JArray() { res };
        }
    }
    public class TxState
    {
        public static TxStateCode TX_SECC = new TxStateCode { code = "0000", codeMessage = "成功" };
        public static TxStateCode TX_FAILD = new TxStateCode { code = "3001", codeMessage = "失败" };
        public static TxStateCode TX_INTERRUPT = new TxStateCode { code = "3002", codeMessage = "中断" };
        public static TxStateCode TX_WAITING = new TxStateCode { code = "3003", codeMessage = "等待" };
        public static TxStateCode TX_NOTFIND = new TxStateCode { code = "3004", codeMessage = "Not find data" };
        public static TxStateCode TX_NULL = new TxStateCode { code = "", codeMessage = "" };
    }
    public class TxStateCode
    {
        public string code { get; set; }
        public string codeMessage { get; set; }
    }
}
