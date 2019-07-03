using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class NNSDomainCreditService
    {
        public mongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }

        public JArray getMappingDomain(string address)
        {
            string findStr = new JObject() { {"address", address } }.ToString();
            string fieldStr = new JObject() { { "fulldomain", 1 }, { "namehash", 1 },{ "ttl", 1 },{ "_id",0} }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, "nnsDomainCreditState", fieldStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray();

            var item = queryRes[0];
            var namehash = item["namehash"].ToString();
            findStr = new JObject() { { "namehash", namehash } }.ToString();
            fieldStr = new JObject() { { "owner",1},{ "TTL",1}, { "_id", 0 } }.ToString();
            queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, "domainOwnerCol", fieldStr, findStr);
            if(queryRes != null && queryRes.Count > 0)
            {
                if (queryRes[0]["owner"].ToString() != address || long.Parse(queryRes[0]["TTL"].ToString()) < TimeHelper.GetTimeStamp())
                {
                    return new JArray();
                }
            }
            return new JArray { new JObject { { "fulldomain", item["fulldomain"]},{"ttl", queryRes[0]["TTL"]} } };
        }
    }
}
