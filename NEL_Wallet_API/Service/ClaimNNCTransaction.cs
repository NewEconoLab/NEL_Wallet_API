using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ThinNeo;

namespace NEL_Wallet_API.Service
{
    public class ClaimNNCTransaction
    {
        public mongoHelper mh { get; set; }
        public string block_mongodbConnStr { get; set; }
        public string block_mongodbDatabase { get; set; }
        public string notify_mongodbConnStr { get; set; }
        public string notify_mongodbDatabase { get; set; }
        public string nncClaimCol { get; set; }
        public AccountInfo accountInfo { get; set; }
        public string nelJsonRpcUrl { get; set; }
        public string hash_nnc { get; set; }
        public string id_gas { get; set; }
        public int batchSendInterval { get; set; } = 1;
        public string isStartFlag { get; set; }


        public void claimNNCLoop()
        {
            if (isStartFlag != "1")
            {
                return;
            }
            new Task(() => processCheckNotifyLoop()).Start();
            while (true)
            {
                try
                {
                    sleep();
                    process();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("claimNNCLoopErrMsg:" + ex.Message);
                    Console.WriteLine("claimNNCLoopErrStack:" + ex.StackTrace);
                }
            }
        }
        private async void process()
        {
            long nowtime = TimeHelper.GetTimeStamp();
            string filter = new JObject() { { "lasttime", new JObject() { { "$gt", nowtime - ClaimNNCState.ONE_DAY_SECONDS } } }, { "state", ClaimNNCState.State_Init } }.ToString();
            string fieldStr = new JObject() { { "address", 1 }, { "amount", 1 } }.ToString();
            string sortStr = new JObject() { { "lasttime", 1 } }.ToString();
            JArray res = mh.GetDataPagesWithField(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, fieldStr, 100/*默认100个*/, 1, sortStr, filter);
            if (res == null || res.Count() == 0) return;

            foreach (var item in res)
            {
                string addr = item["address"].ToString();
                long amount = long.Parse(item["amount"].ToString());
                JObject result = await sendTransaction(addr, amount); ;
                
                //if (result["txcode"].ToString() == "0000")
                {
                    string findStr = new JObject() { { "address", addr } }.ToString();
                    string updateData = new JObject() { { "$set", new JObject() { { "state", ClaimNNCState.State_Processing }, { "txid", result } } } }.ToString();
                    mh.UpdateData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, updateData, findStr);
                }
            }
        }


        private void processCheckNotifyLoop()
        {
            while (true)
            {
                try
                {
                    sleep();
                    checkTxHasInNotify();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("processCheckNotify:" + ex.Message);
                    Console.WriteLine("processCheckNotify:" + ex.StackTrace);
                }
            }
        }

        
        private bool checkTxHasInNotify()
        {
            /**
             * 状态表示：
             * 0 成功
             * 1 已申请
             * 2 已发送
             * 4 交易失败
             * 
             */
            long nowtime = TimeHelper.GetTimeStamp();
            string findStr = new JObject() { { "lasttime", new JObject() { { "$gt", nowtime - ClaimNNCState.ONE_DAY_SECONDS } } }, { "state", ClaimNNCState.State_Processing } }.ToString();
            long cnt = mh.GetDataCount(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, findStr);
            if (cnt == 0) return true;

            string fieldStr = new JObject() { { "txid", 1 }, { "lasttime", 1 },{ "address",1} }.ToString();
            string sortStr = new JObject() { { "lasttime", 1 } }.ToString();
            var query = mh.GetDataPagesWithField(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, fieldStr, 100, 1, sortStr, findStr);
            
            foreach (var item in query)
            {
                string txid = item["txid"]["txid"].ToString();
                long lasttime = long.Parse(item["lasttime"].ToString());

                fieldStr = new JObject() { { "executions", 1 } }.ToString();
                findStr = new JObject() { { "txid", txid } }.ToString();
                var subquery = mh.GetDataWithField(block_mongodbConnStr, block_mongodbDatabase, "notify", fieldStr, findStr);
                if (subquery == null || subquery.Count == 0)
                {
                    if (nowtime > lasttime + ClaimNNCState.ONE_DAY_SECONDS / 2)
                    {
                        // 失败
                        findStr = new JObject() { { "address", item["address"] } }.ToString();
                        string updateData = new JObject() { { "$set", new JObject() { { "state", ClaimNNCState.State_TxFail } } } }.ToString();
                        mh.UpdateData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, updateData, findStr);
                    }
                    continue;
                }

                var exes = (JArray)subquery[0]["executions"];
                if (exes != null && exes.Count > 0)
                {
                    var notifys = (JArray)exes[0]["notifications"];
                    if (notifys != null && notifys.Count > 0)
                    {
                        // 成功
                        findStr = new JObject() { { "address", item["address"] } }.ToString();
                        string updateData = new JObject() { { "$set", new JObject() { { "state", ClaimNNCState.State_TxSucc } } } }.ToString();
                        mh.UpdateData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, updateData, findStr);
                    }
                }
                // continue
            }

            return query.Count < cnt;

        }

        private void sleep()
        {
            for (int i = 0; i < 60; ++i)
            {
                Thread.Sleep(batchSendInterval * 1000);
            }
        }

        public async Task<JObject> sendTransaction(string toAddr, decimal amount, int pricision=2)
        {
            return await sendTransaction(accountInfo.prikey, new Hash160(hash_nnc), 
                "transfer", new string[] {
                    "(addr)" + accountInfo.address,
                    "(addr)" + toAddr,
                    "(int)" + amount +"00"
            });
        }
        public async Task<JObject> sendTransaction(byte[] prikey, Hash160 schash, string method, string[] subparam)
        {
            // 构造合约交易
            byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();

                //
                byte[] randombytes = new byte[32];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randombytes);
                }
                BigInteger randomNum = new BigInteger(randombytes);
                sb.EmitPushNumber(randomNum);
                sb.Emit(ThinNeo.VM.OpCode.DROP);

                for (var i = 0; i < subparam.Length; i++)
                {
                    array.AddArrayValue(subparam[i]);
                }
                sb.EmitParamJson(array);
                sb.EmitPushString(method);
                sb.EmitAppCall(schash);
                data = sb.ToArray();
            }

            // 获取余额
            //string id_nnc = assetid;
            Dictionary<string, List<Utxo>> dir = getBalance(accountInfo.address);
            if (dir == null || dir[id_gas] == null)
            {
                // 余额不足
                return insufficientBalance();
            }

            // 构造并签名
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.InvocationTransaction;
            tran.extdata = new ThinNeo.InvokeTransData { script = data, gas = 0 };
            tran.inputs = new ThinNeo.TransactionInput[0];
            tran.outputs = new ThinNeo.TransactionOutput[0];
            tran.attributes = new ThinNeo.Attribute[1];
            tran.attributes[0] = new ThinNeo.Attribute();
            tran.attributes[0].usage = TransactionAttributeUsage.Script;
            tran.attributes[0].data = ThinNeo.Helper.GetPublicKeyHashFromAddress(accountInfo.address);

            byte[] signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, accountInfo.pubkey, accountInfo.address);
            string txid = tran.GetHash().ToString();
            byte[] transdata = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(transdata);
            
            // 发送交易
            try
            {
                byte[] postdata;
                string url = TransHelper.MakeRpcUrlPost(nelJsonRpcUrl, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
                var result = await TransHelper.HttpPost(url, postdata);
                Console.WriteLine("result:" + result);
                if (JObject.Parse(result)["result"] == null)
                {
                    return txFail(txid);
                }
                JObject res = (JObject)(((JArray)(JObject.Parse(result)["result"]))[0]);
                string flag = res["sendrawtransactionresult"].ToString();
                if (flag.ToLower() != "true" && res["txid"].ToString() != txid)
                {
                    return txFail(txid);
                }
                return txSucc(txid);
            }
            catch (Exception)
            {
                return txFail(txid);
            }
        }

        private Dictionary<string, List<Utxo>> getBalance(string address)
        {
            string findFliter = "{addr:'" + address + "',used:''}";
            JArray result = mh.GetData(block_mongodbConnStr, block_mongodbDatabase, "utxo", findFliter);
            if (result == null || result.Count == 0)
            {
                return null;
            }
            Utxo[] utxoArr = result.Select(p => new Utxo(
                p["addr"].ToString(),
                new ThinNeo.Hash256(p["txid"].ToString()),
                p["asset"].ToString(),
                decimal.Parse(p["value"].ToString(), NumberStyles.Float),
                int.Parse(p["n"].ToString())
                )).ToArray();
            Dictionary<string, List<Utxo>> res = new Dictionary<string, List<Utxo>>();
            foreach (Utxo utxo in utxoArr)
            {
                string assetid = utxo.asset;
                if (res.ContainsKey(assetid))
                {
                    res[assetid].Add(utxo);
                }
                else
                {
                    List<Utxo> list = new List<Utxo>();
                    list.Add(utxo);
                    res[assetid] = list;
                }
            }
            return res;
        }

        private JObject txSucc(string txid)
        {
            return new JObject() { { "code", "tx00" }, { "codeMessage", "交易发送成功" }, { "txid", txid } };
        }
        private JObject txFail(string txid)
        {
            return new JObject() { { "code", "tx01" }, { "codeMessage", "交易发送失败" }, { "txid", txid } };
        }
        private JObject insufficientBalance()
        {
            return new JObject() { { "code", "tx02" }, { "codeMessage", "余额不足" }, { "txid", "" } };
        }
    }
}
