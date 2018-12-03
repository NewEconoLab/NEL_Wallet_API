using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class ClaimNNCService
    {
        public const long ONE_DAY_SECONDS = 24 * 60 * 60;
        public mongoHelper mh { get; set; }
        public string notify_mongodbConnStr { get; set; }
        public string notify_mongodbDatabase { get; set; }
        public string nncClaimCol { get; set; } = "nncClaimCol";
        public int maxClaimAmount { get; set; } = 100;

        public JArray claimNNC(string address, decimal amount=100)
        {
            if (amount > maxClaimAmount || amount <= 0)
            {
                // 超过最大金额
                return new JArray() { overLimitAmount() };
            }
            long nowtime = TimeHelper.GetTimeStamp();
            string filter = new JObject() { { "address", address } }.ToString();
            JArray res = mh.GetData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, filter);
            if (res == null || res.Count() == 0)
            {
                // 从未申请直接入库
                mh.InsertOneData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, new JObject() { { "address", address }, { "amount", amount }, { "lasttime", nowtime }, { "state", "1" }, { "times", 1 }, { "txid", "" } }.ToString());
                return new JArray() { txWait() };
            }
            // 隔天重复申请更新库
            long lasttime = long.Parse(res[0]["lasttime"].ToString());

            long times = res[0]["times"] == null ? 1 : long.Parse(res[0]["times"].ToString());
            if (nowtime - lasttime > ONE_DAY_SECONDS)
            {
                mh.ReplaceData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, filter, new JObject() { { "address", address }, { "amount", amount }, { "lasttime", nowtime }, { "state", "1" }, { "times", times + 1 }, { "txid", "" } }.ToString());
                return new JArray() { txWait() };
            }
            return new JArray() { hasClaimGas() };
        }

        public JArray hasClaimNNC(string address)
        {
            //Boolean flag = true;
            JArray res = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, new JObject() { { "lasttime", 1 }, { "state", 1 } }.ToString(), new JObject() { { "address", address } }.ToString());
            if (res == null || res.Count() == 0)
            {
                // 可领取：从未领取
                return new JArray() { canClaimState() };

            }
            long lasttime = long.Parse(res[0]["lasttime"].ToString());
            long nowtime = TimeHelper.GetTimeStamp();
            if (lasttime < nowtime - ONE_DAY_SECONDS)
            {
                // 可领取：24h后重复领取
                return new JArray() { canClaimState() };
            }

            if (res[0]["state"] == null)
            {
                return new JArray() { hasClaimState() };
            }
            string state = res[0]["state"].ToString();
            if (state == "1" || state == "2" || state == "4")
            {
                // 派发中
                return new JArray() { dealingState() };
            }
            else
            {
                // 已领取
                return new JArray() { hasClaimState() };

            }
        }

        private JObject hasClaimGas()
        {
            return new JObject() { { "code", "3003" }, { "codeMessage", "已经领取" }, { "txid", "" } };
        }
        private JObject overLimitAmount()
        {
            return new JObject() { { "code", "3004" }, { "codeMessage", "超过限制金额" }, { "txid", "" } };
        }
        private JObject txWait()
        {
            return new JObject() { { "code", "3000" }, { "codeMessage", "交易待发送" }, { "txid", "" } };
        }
        private JObject canClaimState()
        {
            return new JObject() { { "code", "3010" }, { "codeMessage", "可领取" }, { "txid", "" } };
        }
        private JObject dealingState()
        {
            return new JObject() { { "code", "3011" }, { "codeMessage", "排队中" }, { "txid", "" } };
        }
        private JObject hasClaimState()
        {
            return new JObject() { { "code", "3012" }, { "codeMessage", "已领取" }, { "txid", "" } };
        }
    }
}
