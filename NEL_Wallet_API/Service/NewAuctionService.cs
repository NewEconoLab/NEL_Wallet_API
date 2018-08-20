using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NEL_Wallet_API.Service
{
    public class NewAuctionService
    {
        public string auctionStateCol { get; set; }
        public mongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }

        public JArray getAuctionInfoByAddress(string address, int pageNum = 1, int pageSize = 10)
        {
            string findStr = new JObject() { { "addwholist.address",address} }.ToString();
            string sortStr = new JObject() { { "startTime.blockindex", -1} }.ToString();
            JArray res = mh.GetDataPages(mongodbConnStr, mongodbDatabase, auctionStateCol, sortStr, pageSize, pageNum, findStr);
            if(res == null || res.Count == 0)
            {
                return new JArray() { };
            }

            return new JArray() { new JObject() { {"count",res.Count },{ "list", res} } };
        }

        public JArray getAuctionInfoByAuctionId(JArray auctionIdsJA, string address = "")
        {
            List<string> list = new List<string>();
            foreach (JValue jv in auctionIdsJA)
            {
                list.Add(jv.ToString());
            }
            return getAuctionInfoByAuctionId(list.ToArray(), address);
        }
        public JArray getAuctionInfoByAuctionId(string[] auctionIdArr, string address = "")
        {
            string findStr = MongoFieldHelper.toFilter(auctionIdArr, "auctionId").ToString();
            JArray res = mh.GetData(mongodbConnStr, mongodbDatabase, auctionStateCol, findStr);
            if (res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            if(address == "")
            {
                return new JArray() { new JObject() { { "count", res.Count }, { "list", res } } };
            }
            foreach(JObject jo in res)
            {
                JArray addwholist = (JArray)jo["addwholist"];
                jo.Remove("addwholist");

                List<JObject> removeList = new List<JObject>();
                foreach (JObject jb in addwholist)
                {
                    if("" != address && jb["address"].ToString() != address)
                    {
                        removeList.Add(jb);
                    }
                }
                if(removeList.Count != 0)
                {
                    foreach(JObject jj in removeList)
                    {
                        addwholist.Remove(jj);
                    }
                }
                jo.Add("addwholist", addwholist);

            }

            return new JArray() { new JObject() { { "count", res.Count }, { "list", res } } };
        }

    }
}
