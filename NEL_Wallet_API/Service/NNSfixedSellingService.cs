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
        public string domainCenterColl { get; set; } = "0xbd3fa97e2bc841292c1e77f9a97a1393d5208b48";
        
        public bool hasNNfixedSelling(string domain, long blockindex, out string owner)
        {
            string findStr = new JObject() { {"fullDomain", domain.ToLower() },{ "blockindex", new JObject() { {"$gte", blockindex } } } }.ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            string fieldStr = new JObject() { { "state", 0 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, 1, 1, sortStr, findStr);
            if(query != null && query.Count > 0)
            {
                string displayName = query[0]["displayName"].ToString();
                if(displayName == "NNSfixedSellingLaunched")
                {
                    owner = query[0]["seller"].ToString();
                    return true;
                }
            }
            owner = "";
            return false;
        }
        public JArray getNNSfixedSellingInfo(string domain)
        {
            domain = domain.ToLower();
            // domain + ttl + price + time
            string namehash = DomainHelper.nameHashFullDomain(domain);
            string findStr = new JObject() { {"namehash", namehash } }.ToString();
            string fieldStr = new JObject() { { "owner",1},{ "TTL",1} }.ToString();
            string sortStr = new JObject() { {"blockindex", -1 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainCenterColl, fieldStr, 1, 1, sortStr, findStr);
            if (query == null || query.Count == 0) return new JArray { };

            string owner = query[0]["owner"].ToString();
            string ttl = query[0]["TTL"].ToString();

            findStr = new JObject() { {"fullDomain", domain } }.ToString();
            fieldStr = new JObject() { {"price",1 }, { "displayName", 1 },{ "seller",1 } }.ToString();
            sortStr = new JObject() { {"blockindex", -1 } }.ToString();
            query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, 1, 1, sortStr, findStr);

            string price = "0";
            string state = "";
            if(query != null && query.Count > 0)
            {
                price = query[0]["price"].ToString();
                if (query[0]["displayName"].ToString() == "NNSfixedSellingLaunched")
                {
                    state = "0901";
                    owner = query[0]["seller"].ToString() ;
                }
            }

            return new JArray
            {
                new JObject() {
                    {"domain", domain },
                    {"owner", owner },
                    {"ttl", ttl },
                    {"price", price },
                    {"state", state },
                }
            };
        }
        public JArray getUpDownBuyInfo(string domain)
        {
            string findStr = new JObject() { { "fullDomain", domain.ToLower() } }.ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            string fieldStr = new JObject() { { "state", 0 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, 1,1, sortStr, findStr);

            return query;
        }
        public JArray getHasBuyListByAddress(string address, string root, int pageNum=1, int pageSize=10)
        {
            
            root = root.StartsWith(".") ? root : "."+root;
            var findJo = newOrFilter("fullDomain", root);
            findJo.Add("seller", address);
            findJo.Add("displayName", "NNSfixedSellingBuy");
            string findStr = findJo.ToString();
            //string findStr = new JObject() { { "seller", address },{ "displayName", "NNSfixedSellingBuy" } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fullDomain", "blockindex", "price" }).ToString();
            string sortStr = new JObject() { {"blockindex",-1 } }.ToString();
            // count
            long count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, findStr);
            if(count == 0) return new JArray { };
            // list
            JObject[] res = new JObject[0];
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, pageSize, pageNum, sortStr,  findStr);
            if (query != null && query.Count > 0)
            {
                var blockindexs = query.Select(p => long.Parse(p["blockindex"].ToString())).Distinct().ToArray();
                var blockindexDict = getBlocktime(blockindexs);

                res = query.Select(p =>
                {
                    JObject jo = (JObject)p;
                    jo.Add("blocktime", blockindexDict.GetValueOrDefault(long.Parse(p["blockindex"].ToString())));
                    return jo;
                }).ToArray();
            }

            return new JArray() { new JObject() { { "count", count}, { "list", new JArray { res } } } };
        }

        private JObject newOrFilter(string key, string regex)
        {
            JObject obj = new JObject();
            JObject subobj = new JObject();
            subobj.Add("$regex", regex);
            subobj.Add("$options", "i");
            obj.Add(key, subobj);
            return obj;
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
