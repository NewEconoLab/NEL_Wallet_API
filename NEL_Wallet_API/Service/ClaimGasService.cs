using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NEL_Wallet_API.Service
{
    public class ClaimGasService
    {
        public const long ONE_DAY_SECONDS = 24 * 60 * 60;
        public string nelJsonRpcUrl { get; set; }
        public string assetid { get; set; }
        public AccountInfo accountInfo { get; set; }
        public string block_mongodbConnStr { get; set; }
        public string block_mongodbDatabase { get; set; }
        public mongoHelper mh { set; get; }
        public string notify_mongodbConnStr { get; set; }
        public string notify_mongodbDatabase { get; set; }
        public string gasClaimCol { get; set; }

        public int batchSendInterval { get; set; }
        public int checkTxInterval { get; set; }
        public int checkTxCount { get; set; }

        public void claimGasLoop()
        {
            while (true)
            {
                Thread.Sleep(batchSendInterval);
                long nowtime = TimeHelper.GetTimeStamp();
                string filter = new JObject() { { "lasttime", new JObject() { { "$gt", nowtime - ONE_DAY_SECONDS } } }, { "state", "1" } }.ToString();
                JArray res = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, gasClaimCol, new JObject() { { "address", 1 }, { "amount", 1 } }.ToString(), filter);
                if (res == null || res.Count() == 0) continue;
                
                // 一笔交易多输出
                mergeClaimGasTx(res.Select(p => p["address"].ToString()).ToList(), long.Parse(res[0]["amount"].ToString()));
            }
        }
        
        private void mergeClaimGasTx(List<string> address, decimal amount)
        {
            TransactionHandler handler = new TransactionHandler();
            handler.nelJsonRpcUrl = nelJsonRpcUrl;
            handler.assetid = assetid;
            handler.accountInfo = accountInfo;
            handler.applyGas(address, amount);
            JObject rr = handler.getResult();
            string code = rr["code"].ToString();
            string txid = rr["txid"].ToString();
            if (code == "0000" || code == "3001")
            {
                // 若入链成功，则更新状态
                if (checkTxHasInBlock(txid))
                {
                    JObject updateData = new JObject() { { "state", "2" } };
                    mh.UpdateData(notify_mongodbConnStr, notify_mongodbDatabase, gasClaimCol, new JObject() { { "$set", updateData } }.ToString(), MongoFieldHelper.toFilter(address.ToArray(), "address").ToString());
                }
            }
        }

        private bool checkTxHasInBlock(string txid)
        {
            bool flag = false;
            long curHeight = getBlockCount();
            do
            {
                if (hasTx(txid))
                {
                    flag = true;
                    break;
                }
                Thread.Sleep(checkTxInterval);
            } while (getBlockCount() <= curHeight + checkTxCount);
            return flag;
        }
        private long getBlockCount()
        {
            return mh.GetDataCount(block_mongodbConnStr, block_mongodbDatabase, "block");
        }
        private bool hasTx(string txid)
        {
            return mh.GetDataCount(block_mongodbConnStr, block_mongodbDatabase, "tx", new JObject() { { "txid", txid } }.ToString()) > 0;
        }
    }
}
