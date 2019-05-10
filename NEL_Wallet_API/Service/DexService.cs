using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;

namespace NEL_Wallet_API.Service
{
    public class DexService
    {
        public mongoHelper mh { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }
        public string dexBalanceStateCol { get; set; }
        public string dexContractHash { get; set; }

        public JArray getBalanceFromDex(string address)
        {
            decimal balance = 0;
            string findStr = new JObject() { {"address", address }, { "contractHash", dexContractHash } }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexBalanceStateCol, findStr);
            if(queryRes != null && queryRes.Count > 0)
            {
                balance = NumberDecimalHelper.formatDecimalDouble(queryRes[0]["balance"].ToString());
            }
            return new JArray { new JObject { { "balance", balance } } };
        }
    }
}
