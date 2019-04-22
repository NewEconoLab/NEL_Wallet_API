using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class MobileService
    {
        public mongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }
        public string domainOwnerCol { get; set; }

        

        public JArray getDomainListByAddress(string address, int pageNum=1, int pageSize=10, string domainPrefix="")
        {
            JObject findJo = new JObject() ;
            if(domainPrefix != "")
            {
                findJo = MongoFieldHelper.likeFilter("fulldomain", domainPrefix); 
            }
            findJo.Add("owner", address);
            findJo.Add("TTL", new JObject() { { "$lte", TimeHelper.GetTimeStamp() } });
            
            //
            string findStr = findJo.ToString();
            string fieldStr = new JObject() { {"fulldomain", 1 }, { "bindflag",1} }.ToString();
            string sortStr = new JObject() { {"blockindex", -1} }.ToString();
            var queryRes = mh.GetDataPagesWithField(mongodbConnStr, mongodbDatabase, domainOwnerCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            //
            long count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, domainOwnerCol, findStr);

            var res = queryRes.OrderByDescending(p => long.Parse(p["bindflag"].ToString())).ToArray();

            

            return new JArray
            { new JObject(){ {"count", count }, { "list", new JArray { res } } }
            };
        }
    }
}
