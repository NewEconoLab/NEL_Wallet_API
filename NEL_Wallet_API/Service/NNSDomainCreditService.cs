using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;

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
            string fieldStr = new JObject() { {"fulldomain", 1 },{ "ttl", 1 },{ "_id",0} }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, "nnsDomainCreditState", fieldStr, findStr);
            return queryRes;
        }
    }
}
