using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;

namespace NEL_Wallet_API.Service
{
    public class CommonService
    {
        public mongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }

        public JArray getTransByAddress(string address, int pageSize, int pageNum)
        {
            JObject filter = new JObject() { { "addr", address } };
            JObject sort = new JObject() { { "blockindex", -1 }, { "txid", -1 } };
            JArray result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address_tx", sort.ToString(), pageSize, pageNum, filter.ToString());
            if(result == null || result.Count == 0)
            {
                return new JArray() { };
            }
            return result;
        }   
    }
}
