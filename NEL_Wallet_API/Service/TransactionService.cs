using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NEL_Wallet_API.Service
{
    public class TransactionService
    {
        public const long ONE_DAY_SECONDS = 24 * 60 * 60;
        public string nelJsonRpcUrl { get; set; }
        public string assetid { get; set; }
        public AccountInfo accountInfo { get; set; }
        public mongoHelper mh { set; get; }
        public string notify_mongodbConnStr { get; set; }
        public string notify_mongodbDatabase { get; set; }
        public string gasClaimCol { get; set; }


        /// <summary>
        /// 
        /// 0000 成功(表示发送交易成功)
        /// 3001 交易发送失败
        /// 3002 余额不足
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public JArray claimGas(string address, decimal amount = 1)
        {
            TransactionHandler handler = new TransactionHandler();
            handler.nelJsonRpcUrl = nelJsonRpcUrl;
            handler.assetid = assetid;
            handler.accountInfo = accountInfo;
            handler.applyGas(address, amount);
            JObject res = handler.getResult();
            // 成功则写入库标记(一天只能领取一次)
            if(res["code"].ToString() == "0000")
            {
                JObject jo = new JObject() { { "address", address}, { "amount", amount }, { "lasttime", TimeHelper.GetTimeStamp() } };
                mh.ReplaceOrInsertData(notify_mongodbConnStr, notify_mongodbDatabase, gasClaimCol, new JObject() { { "address", address } }.ToString(), jo.ToString());
            }
            return new JArray() { res };
        }

        public JArray hasClaimGas(string address)
        {
            Boolean flag = true;
            JArray res = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, gasClaimCol, new JObject() { { "lasttime", 1 } }.ToString(), new JObject() { { "address", address } }.ToString());
            if(res != null && res.Count() != 0)
            {
                long lasttime = long.Parse(res[0]["lasttime"].ToString());
                long nowtime = TimeHelper.GetTimeStamp();
                if(nowtime < lasttime  + ONE_DAY_SECONDS)
                {
                    flag = false;
                }
            }
            return new JArray() { new JObject() { { "flag", flag} } }; ;
        }

    }
    public class TransactionHandler
    {
        public string nelJsonRpcUrl { get; set; }
        public string assetid { get; set; }
        public AccountInfo accountInfo { get; set; }
        private JObject res;
        
        public JObject getResult()
        {
            while(res == null)
            {
                Thread.Sleep(100);
            }
            return res;
        }
        public async void applyGas(string address, decimal amount = 1)
        {
            res = await asyncApplyGas(address, amount);
        }
        private async Task<JObject> asyncApplyGas(string targetAddress, decimal amount)
        {
            // 转换私钥
            byte[] prikey = accountInfo.prikey;
            byte[] pubkey = accountInfo.pubkey;
            string address = accountInfo.address;

            // 获取余额
            string id_gas = assetid;
            Dictionary<string, List<Utxo>> dir = await TransHelper.GetBalanceByAddress(nelJsonRpcUrl, address);
            List<Utxo> balanceUtxo = dir[id_gas];
            if (balanceUtxo.Sum(p => p.value) < amount)
            {
                // 余额不足
                return insufficientBalance();
            }

            // 构造并发送交易
            ThinNeo.Transaction tran = TransHelper.makeTran(dir[id_gas], targetAddress, new ThinNeo.Hash256(id_gas), amount);
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            byte[] msg = tran.GetMessage();
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
            tran.AddWitness(signdata, pubkey, address);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);
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
            if (flag != "True" && res["txid"].ToString() != txid)
            {
                return txFail(txid);
            }
            return txSucc(txid);
        }

        private JObject txSucc(string txid)
        {
            return new JObject() { { "code", "0000" }, { "codeMessage", "交易发送成功" }, { "txid", txid } };
        }
        private JObject txFail(string txid)
        {
            return new JObject() { { "code", "3001" }, { "codeMessage", "交易发送失败" }, { "txid", txid } };
        }
        private JObject insufficientBalance()
        {
            return new JObject() { { "code", "3002" }, { "codeMessage", "余额不足" }, { "txid", "" } };
        }
    }
}
