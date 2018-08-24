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
            JObject stateFilter = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_START, AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM, AuctionState.STATE_END }, "auctionState");
            JObject addressFilter = new JObject() { {"$or", new JArray() { new JObject() { { "addwholist.address", address } }, new JObject() { { "startAddress", address } }, new JObject() { { "endAddress", address } } } } };
            string findStr = new JObject() { { "$and", new JArray() { stateFilter, addressFilter } } }.ToString();
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
            //string findStr = MongoFieldHelper.toFilter(auctionIdArr, "auctionId").ToString();
            JObject auctionIdFilter = MongoFieldHelper.toFilter(auctionIdArr, "auctionId");
            JObject addressFilter = new JObject() { { "$or", new JArray() { new JObject() { { "addwholist.address", address } }, new JObject() { { "startAddress", address } }, new JObject() { { "endAddress", address } } } } };
            string findStr = new JObject() { { "$and", new JArray() { auctionIdFilter, addressFilter } } }.ToString();
            JArray res = mh.GetData(mongodbConnStr, mongodbDatabase, auctionStateCol, findStr);
            if (res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            if(address == "")
            {
                return new JArray() { new JObject() { { "count", res.Count }, { "list", res } } };
            }
            foreach (JObject jo in res)
            {
                if (jo["addwholist"] == null || jo["addwholist"].ToString() == "" || ((JArray)jo["addwholist"]).Count == 0)
                {
                    continue;
                }
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
    class AuctionState
    {
        public const string STATE_START = "0101";
        public const string STATE_CONFIRM = "0201";
        public const string STATE_RANDOM = "0301";
        public const string STATE_END = "0401"; // 触发结束、3D/5D到期结束
        public const string STATE_ABORT = "0501";
        public const string STATE_EXPIRED = "0601";
    }
}
