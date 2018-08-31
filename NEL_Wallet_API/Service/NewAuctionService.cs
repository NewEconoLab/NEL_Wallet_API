using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class NewAuctionService
    {
        public string auctionStateCol { get; set; }
        public mongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }
        private const long ONE_DAY_SECONDS = 1 * /*24 * 60 * */60 /*测试时5分钟一天*/* 5;
        private const long ONE_YEAR_SECONDS = ONE_DAY_SECONDS * 365;

        public JArray getAcutionInfoCount(string address)
        {
            JObject stateFilter = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_START, AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM, AuctionState.STATE_END }, "auctionState");
            JObject addressFilter = new JObject() { { "$or", new JArray() { new JObject() { { "addwholist.address", address } }, new JObject() { { "startAddress", address } }, new JObject() { { "endAddress", address } } } } };
            string findStr = new JObject() { { "$and", new JArray() { stateFilter, addressFilter } } }.ToString();
            long count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, auctionStateCol, findStr);

            return new JArray() { new JObject() { { "count", count } } };
        }
        public JArray getAuctionInfoByAddress(string address, int pageNum = 1, int pageSize = 10)
        {
            JObject stateFilter = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_START, AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM, AuctionState.STATE_END }, "auctionState");
            JObject addressFilter = new JObject() { {"$or", new JArray() { new JObject() { { "addwholist.address", address } }, new JObject() { { "startAddress", address } }, new JObject() { { "endAddress", address } } } } };
            //JObject expireFilter = new JObject() { {"startTime.blocktime", new JObject() { {"$gt",  TimeHelper.GetTimeStamp() - ONE_YEAR_SECONDS } } } };
            //string findStr = new JObject() { { "$and", new JArray() { stateFilter, addressFilter, expireFilter } } }.ToString();
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
            string[] auctionIdArr = auctionIdsJA.Select(p => p.ToString().StartsWith("0x") ? p.ToString() : "0x" + p.ToString()).ToArray();
            return getAuctionInfoByAuctionId(auctionIdArr, address);
        }
        public JArray getAuctionInfoByAuctionId(string[] auctionIdArr, string address = "")
        {
            //string findStr = MongoFieldHelper.toFilter(auctionIdArr, "auctionId").ToString();
            string findStr = null;
            if (address!= null && address != "")
            {
                JObject auctionIdFilter = MongoFieldHelper.toFilter(auctionIdArr, "auctionId");
                JObject addressFilter = new JObject() { { "$or", new JArray() { new JObject() { { "addwholist.address", address } }, new JObject() { { "startAddress", address } }, new JObject() { { "endAddress", address } } } } };
                findStr = new JObject() { { "$and", new JArray() { auctionIdFilter, addressFilter } } }.ToString();
            } else
            {
                findStr = MongoFieldHelper.toFilter(auctionIdArr, "auctionId").ToString();
            }
           
            JArray res = mh.GetData(mongodbConnStr, mongodbDatabase, auctionStateCol, findStr);
            if (res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            /*
            // 过期与否判断
            foreach (JObject jo in res)
            {
                string auctionState = jo["auctionState"].ToString();
                if (auctionState == "0401")
                {
                    long startBlocktime = long.Parse(jo["startTime"]["blocktime"].ToString());
                    if (startBlocktime <= TimeHelper.GetTimeStamp() - ONE_YEAR_SECONDS)
                    {
                        jo.Remove("auctionState");
                        jo.Add("auctionState", "0601");
                    }
                }
            }
            */

            if (address == "")
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
