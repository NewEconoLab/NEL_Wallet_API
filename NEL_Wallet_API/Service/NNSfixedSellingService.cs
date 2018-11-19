using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEL_Wallet_API.Service
{
    public class NNSfixedSellingService
    {
        public mongoHelper mh { get; set; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }

        public string NNSfixedSellingColl { get; set; } = "0x7a64879a21b80e96a8bc91e0f07adc49b8f3521e";

        public JArray getHasBuyListByAddress(string address)
        {
            string findStr = new JObject() { { "seller", address },{ "displayName", "NNSfixedSellingBuy" } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fullDomain", "blockindex", "price" }).ToString();
            var query = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, findStr);
            if (query == null || query.Count == 0) return new JArray { };

            var blockindexs = query.Select(p => long.Parse(p["blockindex"].ToString())).Distinct().ToArray();
            var blockindexDict = getBlocktime(blockindexs);

            var res = query.Select(p =>
            {
                JObject jo = (JObject)p;
                jo.Add("blocktime", blockindexDict.GetValueOrDefault(long.Parse(p["blockindex"].ToString())));
                return jo;
            }).ToArray();

            return new JArray() { { res} };
        }

        private Dictionary<long, long> getBlocktime(long[] indexs)
        {
            if (indexs == null &&  indexs.Length == 0) return null;

            string findStr = MongoFieldHelper.toFilter(indexs, "index").ToString() ;
            string fieldStr = new JObject() { {"index", 1 }, { "time", 1 } }.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
            return query.ToDictionary(k => long.Parse(k["index"].ToString()), v => long.Parse(v["time"].ToString()));
        }
    }
}
