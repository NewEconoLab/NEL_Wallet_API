using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class ClaimNNCService
    {
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
                return new JArray() { ClaimNNCState.PR_OverLimitAmountState };
            }
            
            long nowtime = TimeHelper.GetTimeStamp();
            string filter = new JObject() { { "address", address } }.ToString();
            JArray res = mh.GetData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, filter);
            if (res == null || res.Count() == 0)
            {
                // 从未申请直接入库
                mh.InsertOneData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, new JObject() { { "address", address }, { "amount", amount }, { "lasttime", nowtime }, { "state", ClaimNNCState.State_Init }, { "times", 1 }, { "txid", "" } }.ToString());
                return new JArray() { ClaimNNCState.PR_ProcessingState };
            }
            
            long lasttime = long.Parse(res[0]["lasttime"].ToString());
            long times = res[0]["times"] == null ? 1 : long.Parse(res[0]["times"].ToString());
            if (nowtime - lasttime > ClaimNNCState.ONE_DAY_SECONDS)
            {
                // 隔天重复申请更新库
                mh.ReplaceData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, filter, new JObject() { { "address", address }, { "amount", amount }, { "lasttime", nowtime }, { "state", ClaimNNCState.State_Init }, { "times", times + 1 }, { "txid", "" } }.ToString());
                return new JArray() { ClaimNNCState.PR_ProcessingState };
            }

            string state = res[0]["state"].ToString();
            if(state == ClaimNNCState.State_TxFail)
            {
                // <24h,申请失败重试更新库
                mh.ReplaceData(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, filter, new JObject() { { "address", address }, { "amount", amount }, { "lasttime", nowtime }, { "state", ClaimNNCState.State_Init }, { "times", times + 1 }, { "txid", "" } }.ToString());
                return new JArray() { ClaimNNCState.PR_ProcessingState };
            }
            if (state == ClaimNNCState.State_TxSucc)
            {
                // <24h, 成功
                return new JArray() { ClaimNNCState.PR_HasClaimState };
            }
            // <24h, 其他(Init + Processing)
            return new JArray() { ClaimNNCState.PR_ProcessingState };

        }

        public JArray hasClaimNNC(string address)
        {
            //Boolean flag = true;
            JArray res = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, nncClaimCol, new JObject() { { "lasttime", 1 }, { "state", 1 } }.ToString(), new JObject() { { "address", address } }.ToString());
            if (res == null || res.Count() == 0)
            {
                // 可领取：从未领取
                return new JArray() { ClaimNNCState.PR_CanClaimState };

            }
            long lasttime = long.Parse(res[0]["lasttime"].ToString());
            long nowtime = TimeHelper.GetTimeStamp();
            if (lasttime < nowtime - ClaimNNCState.ONE_DAY_SECONDS)
            {
                // 可领取：24h后重复领取
                return new JArray() { ClaimNNCState.PR_CanClaimState };
            }
            
            string state = res[0]["state"].ToString();
            if(state == ClaimNNCState.State_TxFail)
            {
                // <24h, 可再次领取
                return new JArray() { ClaimNNCState.PR_CanClaimAgainState };
            }
            if (state == ClaimNNCState.State_Init || state == ClaimNNCState.State_Processing)
            {
                // <24h, 处理中
                return new JArray() { ClaimNNCState.PR_ProcessingState };
            }
            else
            {
                // <24h, 已领取
                return new JArray() { ClaimNNCState.PR_HasClaimState };
            }
        }
    }

}
class ClaimNNCState
{
    /**
     * 一天的秒数
     */
    public const long ONE_DAY_SECONDS = 24 * 60 * 60;

    /**
     * 前端页面需要状态码
     */
    public static JObject PR_CanClaimState = new JObject() { { "code", "3001" }, { "codeMessage", "可领取" }, { "txid", "" } };
    public static JObject PR_CanClaimAgainState = new JObject() { { "code", "3002" }, { "codeMessage", "可再次领取" }, { "txid", "" } };
    public static JObject PR_ProcessingState = new JObject() { { "code", "3003" }, { "codeMessage", "处理中" }, { "txid", "" } };
    public static JObject PR_HasClaimState = new JObject() { { "code", "3004" }, { "codeMessage", "已领取" }, { "txid", "" } };
    public static JObject PR_OverLimitAmountState = new JObject() { { "code", "3011" }, { "codeMessage", "超出限额" }, { "txid", "" } };
    public static JObject PR_IinsufficientBalanceState = new JObject() { { "code", "3012" }, { "codeMessage", "余额不足" }, { "txid", "" } };

    /**
     * 后端申请状态记录
     */
    public static string State_Init = "1";
    public static string State_Processing = "2";
    public static string State_ = "3";
    public static string State_TxFail = "4";
    public static string State_TxSucc = "0";
     
}
