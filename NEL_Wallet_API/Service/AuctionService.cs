using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using ThinNeo;

namespace NEL_Wallet_API.Controllers
{
    public class AuctionService
    {
        private long THREE_DAY_SECONDS = 3 * /*24 * 60 * */60 /*测试时5分钟一天*/* 5;
        private long TWO_DAY_SECONDS = 2 * /*24 * 60 * */60 /*测试时5分钟一天*/* 5;
        private long ONE_DAY_SECONDS = 1 * /*24 * 60 * */60 /*测试时5分钟一天*/* 5;
        private long FIVE_DAY_SECONDS = 5 * /*24 * 60 * */60 /*测试时5分钟一天*/ * 5;
        private long DAYS_OF_YEAR = 365;
        public string Notify_mongodbConnStr { set; get; }
        public string Notify_mongodbDatabase { set; get; }
        public mongoHelper mh { set; get; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string queryDomainCollection { get; set; }
        public string queryBidListCollection { get; set; }
        public AuctionRecharge auctionRecharge { get; set; }


        public JArray hasTx(string txid)
        {
            bool issucces = queryHasTxFromBlock("tx", new JObject() { { "txid", txid } }.ToString());

            return new JArray() { { new JObject() { { "issucces", issucces } } } };
        }

        public JArray hasContract(string txid)
        {
            JArray queryRes = queryNotifyFromBlock("notify", new JObject() { { "txid", txid } }.ToString());
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { new JObject() { { "displayNameList", new JArray() { } } } };
            }
            string[] res = queryRes.Where(p => ((JArray)p["notifications"]).Count() != 0).SelectMany(p =>
            {
                JArray pArr = (JArray)p["notifications"];
                return pArr.Select(pp => pp["state"]["value"][0]["value"].ToString()).Select(pp => pp.Hexstring2String()).ToArray();
            }).ToArray();
            return new JArray() { new JObject() { { "displayNameList", new JArray() { res } } } };
        }

        public JArray rechargeAndTransfer(string txhex1, string txhex2)
        {
            return auctionRecharge.rechargeAndTransfer(txhex1, txhex2);
        }

        public JArray getRechargeAndTransfer(string txid)
        {
            return auctionRecharge.getRechargeAndTransfer(txid);
        }

        public JArray getDomainState(string address, /*string domain*/ string auctionid)
        {
            JObject filter = new JObject();
            filter.Add("id", auctionid);
            filter.Add("displayName", "addprice");

            JObject fieldFilter = new JObject() { { "maxBuyer", 1 }, { "maxPrice", 1 }, { "startBlockSelling", 1 }, { "who", 1 }, { "value", 1 } };
            JArray maxPriceObj = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, queryBidListCollection, fieldFilter.ToString(), filter.ToString());
            if(maxPriceObj == null || maxPriceObj.Count == 0)
            {
                return new JArray() { };
            }
            JObject obj = (JObject)maxPriceObj[0];
            string maxBuyer = obj["maxBuyer"].ToString();
            string maxPrice = obj["maxPrice"].ToString();
            string mybidprice = maxPrice;
            if (address != maxBuyer)
            {
                mybidprice = maxPriceObj.Where(p => p["who"].ToString() == address).Sum(p => double.Parse(p["value"].ToString())).ToString();
            }
            return new JArray() { { new JObject() { { "id", auctionid }, { "maxBuyer", maxBuyer }, { "maxPrice", maxPrice }, { "mybidprice", mybidprice } } } };
        }
        
        private bool queryHasTxFromBlock(string coll, string filter)
        {
            return mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, coll, filter) >= 1;
        }
        private JArray queryNotifyFromBlock(string coll, string filter)
        {
            return mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, coll, filter);
        }


        public JArray getBidListByAddressLikeDomain(string address, string prefixDomain, int pageNum = 1, int pageSize = 10)
        {
            return queryBidListByAddress(address, pageNum, pageSize, prefixDomain);
        }
        public JArray getBidListByAddress(string address, int pageNum = 1, int pageSize = 10)
        {
            return queryBidListByAddress(address, pageNum, pageSize);
        }

        public string domainUserStateCol { get; set; }
        public string domainStateCol { get; set; }
       
        private JArray queryBidListByAddress(string address, int pageNum, int pageSize, string prefixDomain="")
        {
            // 地址参拍域名
            JObject domainFilter = new JObject() { { "who", address }};
            if (prefixDomain != "")
            {
                domainFilter.Add("fulldomain", new JObject() { { "$regex", prefixDomain }, { "$options", "i" } });
            }
            JArray domainRes = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainUserStateCol, new JObject() { {"fulldomain",1 } }.ToString(), domainFilter.ToString());
            if (domainRes == null || domainRes.Count() == 0)
            {
                return new JArray() { };
            }

            // 域名当前状态
            JObject queryFilter = new JObject(); queryFilter.Add("$and", new JArray() { MongoFieldHelper.toFilter(domainRes.ToArray().Select(p => p["fulldomain"].ToString()).ToArray(), "fulldomain"), new JObject() { { "auctionState", new JObject() { { "$ne", "4"} } } } });
            JObject querySort = new JObject() { { "blockindex", -1 } };
            JObject queryField = MongoFieldHelper.toReturn(new string[] { "fulldomain", "startBlockSellingTime","auctionState","maxPrice","maxBuyer","endBlock","blockindex","id","owner","auctionSpentTime"});
            JArray res = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainStateCol, queryField.ToString(), pageSize, pageNum, querySort.ToString(), queryFilter.ToString());
            if (res == null || res.Count() == 0)
            {
                return new JArray() { };
            }
            // 域名总量
            long cnt = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, domainStateCol, queryFilter.ToString());
            JObject rr = new JObject();
            rr.Add("list", new JArray() { res.Select(p => {
                JObject jo = (JObject)p;
                jo.Add("domain", jo["fulldomain"].ToString());
                jo.Add("startAuctionTime", jo["startBlockSellingTime"].ToString());
                jo.Remove("fulldomain");
                jo.Remove("startBlockSellingTime");
                string auctionState = jo["auctionState"].ToString();
                if(auctionState == "3" || auctionState == "5")
                {
                    jo.Remove("auctionState");
                    jo.Add("auctionState", "0");
                }
                return jo;
            }).ToArray() });
            rr.Add("count", cnt);
            
            return new JArray() { rr };
        }

        public JArray getBidDetailByAuctionId(string auctionId, int pageNum = 1, int pageSize = 10)
        {
            if(!auctionId.StartsWith("0x"))
            {
                auctionId = "0x" + auctionId;
            }
            JObject filter = new JObject();
            filter.Add("id", auctionId);
            filter.Add("displayName", "addprice");
            filter.Add("maxPrice", new JObject() {{ "$ne", "0" }});
            // 累加value需要查询所有记录
            JArray queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, queryBidListCollection, filter.ToString());
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }
            // 批量查询blockindex对应的时间
            long[] blockindexArr = queryRes.Select(item => long.Parse(item["blockindex"].ToString())).ToArray();
            Dictionary<string, long> blocktimeDict = getBlockTime(blockindexArr);
            // 
            JObject[] arr = queryRes.Select(item =>
            {
                string maxPrice = item["maxPrice"].ToString();
                string maxBuyer = item["maxBuyer"].ToString();
                string who = item["who"].ToString();
                if (maxBuyer != who)
                {
                    maxBuyer = who;
                    maxPrice = Convert.ToString(queryRes.Where(pItem => pItem["who"].ToString() == who && int.Parse(pItem["blockindex"].ToString()) <= int.Parse(item["blockindex"].ToString())).Sum(ppItem => double.Parse(ppItem["value"].ToString())));
                }
                long addPriceTime = blocktimeDict.GetValueOrDefault(Convert.ToString(item["blockindex"]));
                // 新增txid +出价人 +当笔出价金额
                string txid = item["txid"].ToString();
                string bidder = item["who"].ToString();
                double raisebid = double.Parse(item["value"].ToString());
                return new JObject() { { "maxPrice", maxPrice }, { "maxBuyer", maxBuyer }, { "addPriceTime", addPriceTime }, { "txid", txid }, { "bidder", bidder }, { "raisebid", raisebid } };

            }).OrderByDescending(p => double.Parse(p["maxPrice"].ToString())).ThenByDescending(p => p["addPriceTime"]).ToArray();
            // 返回
            JObject res = new JObject();
            res.Add("count", arr.Count());
            res.Add("list", new JArray() { arr.Skip(pageSize*(pageNum-1)).Take(pageSize).ToArray() });
            return new JArray() { res };
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
