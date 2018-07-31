using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Wallet_API.Controllers
{
    public class BonusService
    {
        public string Notify_mongodbConnStr { set; get; }
        public string Notify_mongodbDatabase { set; get; }
        public mongoHelper mh { set; get; }
        public string BonusNofityCol { set; get; }
        public string BonusNofityFrom { set; get; }
        public string Block_mongodbConnStr { set; get; }
        public string Block_mongodbDatabase { set; get; }


        public JArray getBonusHistByAddress(string address, int pageNum = 1, int pageSize = 10)
        {
            JObject queryFilter = new JObject() { { "from", BonusNofityFrom }, { "to", address } };
            string querySort = "{\"blockindex\":-1,\"txid\":-1}";
            JObject queryField = new JObject() { { "value", 1 }, { "blockindex", 1 } };
            JArray queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, BonusNofityCol, queryField.ToString(), pageSize, pageNum, querySort, queryFilter.ToString());
            if (queryRes == null || queryRes.Count() == 0)
            {
                return new JArray() { };
            }

            //
            long[] blockindexArr = queryRes.Select(p => long.Parse(p["blockindex"].ToString())).Distinct().ToArray();
            Dictionary<string, long> blockTimeDict = getBlockTime(blockindexArr);

            // 
            JObject[] arr = queryRes.Select(item =>
            {
                JObject obj = new JObject();
                obj.Add("value", item["value"]);
                obj.Add("blocktime", blockTimeDict.GetValueOrDefault(item["blockindex"].ToString()));
                return obj;
            }).ToArray();

            // 总量
            long cnt = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, BonusNofityCol, queryFilter.ToString());
            // 返回
            JObject res = new JObject();
            res.Add("list", new JArray() { arr });
            res.Add("count", cnt);
            return new JArray() { res };

        }
        
        private long getBlockTime(string blockHeightStrSt)
        {
            string blockHeightFilter = "{\"index\":" + long.Parse(blockHeightStrSt) + "}";
            JArray queryBlockRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "block", blockHeightFilter);
            long blockTime = long.Parse(Convert.ToString(queryBlockRes[0]["time"]));
            return blockTime;
        }
        private Dictionary<string, long> getBlockTime(long[] blockindexArr)
        {
            JObject queryFilter = MongoFieldHelper.toFilter(blockindexArr, "index", "$or");
            JObject returnFilter = MongoFieldHelper.toReturn(new string[] { "index", "time" });
            JArray blocktimeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", returnFilter.ToString(), queryFilter.ToString());
            return blocktimeRes.ToDictionary(key => key["index"].ToString(), val => long.Parse(val["time"].ToString()));
        }
    }
}
