using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class DexService
    {
        public mongoHelper mh { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }
        public string dexStarStateCol { get; set; } = "dexStarState";
        public string domainOwnerCol { get; set; } = "domainOwnerCol";
        public string auctionStateCol { get; set; } = "auctionState";
        public string dexBalanceStateCol { get; set; }
        public string dexDomainSellStateCol { get; set; }
        public string dexDomainBuyStateCol { get; set; }
        public string dexDomainDealHistStateCol { get; set; }
        public string dexContractHash { get; set; }
        public long newlyDataTimeRange { get; set; } = 3 * 24 * 60 * 60; // 3天内
        public long nowTime => TimeHelper.GetTimeStamp();

        public JArray getBalanceFromDex(string address, string assetHash="")
        {
            JObject findJo = new JObject() { {"address", address } };
            if(assetHash != "")
            {
                findJo.Add("assetHash", assetHash);
            }
            string findStr = findJo.ToString();

            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexBalanceStateCol, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var res = queryRes.Select(p =>
            {
                return new JObject {
                    {"assetHash", p["assetHash"] },
                    {"assetName", p["assetName"] },
                    {"balance", NumberDecimalHelper.formatDecimalDouble(p["balance"].ToString()) }
                };
            }).ToArray();
            return new JArray { new JObject {
                {"count",res.Count() },
                { "list", new JArray{ res } }
            } };
        }

        public JArray getDexDomainSellList(string address, int pageNum=1, int pageSize=10, string sortType= ""/*newtime(startTimeStamp).priceH(nowPrice).priceL.starCount.mortgate*/, string assetFilterType=""/*all/cgas/nnc*/, string starFilterType=""/*all/mine/other*/)
        {
            string findStr = getFindStr(assetFilterType, starFilterType, true);
            string sortStr = getSortStr(sortType, "sell");
            string fieldStr = new JObject { {"fullDomain", 1 }, { "owner", 1 }, { "ttl", 1 }, { "starCount", 1 }, { "assetName", 1 }, { "nowPrice", 1 }, { "saleRate", 1 }, { "sellType", 1 }, { "_id",0} }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, findStr);
            if (count == 0) return new JArray { };
            
            var queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            var res = queryRes.Select(p =>
            {
                var jo = (JObject)p;
                var tmp = NumberDecimalHelper.formatDecimal(jo["nowPrice"].ToString());
                jo.Remove("nowPrice") ;
                jo.Add("nowPrice", tmp);
                tmp = NumberDecimalHelper.formatDecimal(jo["saleRate"].ToString());
                jo.Remove("saleRate");
                jo.Add("saleRate", tmp);

                jo.Add("isMine", jo["owner"].ToString() == address);
                jo.Remove("owner");
                jo.Add("isStar", false);
                jo.Remove("starCount");
                return jo;
            }).ToArray();
            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }
        public JArray getDexDomainBuyList(string address, int pageNum = 1, int pageSize = 10, string sortType = ""/*newtime(maxTime).priceH(maxPrice).priceL.starCouunt*/, string assetFilterType = ""/*all/cgas/nnc*/, string starFilterType = ""/*all/mine/other*/)
        {
            string findStr = getFindStr(assetFilterType, starFilterType);
            string sortStr = getSortStr(sortType, "buy");
            string fieldStr = new JObject { { "fullDomain", 1 }, {"buyer",1 },{ "assetName", 1 }, { "price", 1 }, { "time", 1 }, { "owner", 1 }, { "starCount", 1 }, { "_id", 0 } }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr);
            if (count == 0) return new JArray { };

            var queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            var res = queryRes.Select(p =>
            {
                var jo = (JObject)p;
                var tmp = NumberDecimalHelper.formatDecimal(jo["price"].ToString());
                jo.Remove("price");
                jo.Add("price", tmp);
                jo.Add("isNewly", nowTime < long.Parse(jo["time"].ToString()) + newlyDataTimeRange);
                jo.Remove("time");
                jo.Add("canSell", jo["owner"].ToString() == address);
                jo.Remove("owner");
                jo.Add("isStar", false);
                jo.Remove("starCount");
                
                return jo;
            }).ToArray();
            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }
        public JArray getDexDomainDealHistList(string address, int pageNum = 1, int pageSize = 10, string sortType = ""/*newtime(blocktime).priceH(price).priceL*/, string assetFilterType="")
        {
            string findStr = getFindStr(assetFilterType, "");
            string sortStr = getSortStr(sortType, "deal");
            string fieldStr = new JObject { { "fullDomain", 1 }, { "price", 1 }, { "assetName", 1 }, { "_id", 0 } }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainDealHistStateCol, findStr);
            if (count == 0) return new JArray { };

            var queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainDealHistStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            var res = queryRes.Select(p =>
            {
                var jo = (JObject)p;
                var tmp = NumberDecimalHelper.formatDecimal(jo["price"].ToString());
                jo.Remove("price");
                jo.Add("price", tmp);
                return jo;
            }).ToArray();
            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }

        private string getFindStr(string assetFilterType, string starFilterType, bool NotExpired=false)
        {

            if (starFilterType == SortFilterType.StarFilter_Mine)
            {
                // TODO
            }

            if (assetFilterType == "" && starFilterType == "")
            {
                if(NotExpired)
                {
                    return new JObject { {"ttl", new JObject { {"$gte", TimeHelper.GetTimeStamp() } } } }.ToString();
                }
                return "{}";
            }

            assetFilterType = assetFilterType.ToUpper();
            JObject findJo = new JObject();
            if(assetFilterType == SortFilterType.AssetFilter_CGAS || assetFilterType == SortFilterType.AssetFilter_NNC)
            {
                findJo.Add("assetName", assetFilterType);
            }

            if(NotExpired)
            {
                findJo.Add("ttl", new JObject { { "$gte", TimeHelper.GetTimeStamp()} });
            }

            return findJo.ToString();
        }
        private string getSortStr(string sortType, string sellOrBuyOrDeal)
        {
            if (sortType == "") return "{}";
            switch(sortType)
            {
                case SortFilterType.Sort_MortgagePayments:
                case SortFilterType.Sort_MortgagePayments_High:
                    return new JObject { { "mortgagePayments", -1 } }.ToString();
                case SortFilterType.Sort_MortgagePayments_Low:
                    return new JObject { { "mortgagePayments", 1 } }.ToString();
                case SortFilterType.Sort_LaunchTime:
                case SortFilterType.Sort_LaunchTime_New:
                    if (sellOrBuyOrDeal == "buy")
                    {
                        return new JObject { { "maxTime", -1 } }.ToString();
                    }
                    if (sellOrBuyOrDeal == "deal")
                    {
                        return new JObject { { "blocktime", -1 } }.ToString();
                    }
                    return new JObject { { "startTimeStamp", -1 } }.ToString();
                case SortFilterType.Sort_LaunchTime_Old:
                    if (sellOrBuyOrDeal == "buy")
                    {
                        return new JObject { { "maxTime", 1 } }.ToString();
                    }
                    if (sellOrBuyOrDeal == "deal")
                    {
                        return new JObject { { "blocktime", 1 } }.ToString();
                    }
                    return new JObject { { "startTimeStamp", 1 } }.ToString();
                case SortFilterType.Sort_Price:
                case SortFilterType.Sort_Price_High:
                    if (sellOrBuyOrDeal == "buy")
                    {
                        return new JObject { { "maxPrice", -1 } }.ToString();
                    }
                    if (sellOrBuyOrDeal == "deal")
                    {
                        return new JObject { { "price", -1 } }.ToString();
                    }
                    return new JObject { { "nowPrice", -1 } }.ToString();
                case SortFilterType.Sort_Price_Low:
                    if (sellOrBuyOrDeal == "buy")
                    {
                        return new JObject { { "maxPrice", 1 } }.ToString();
                    }
                    if (sellOrBuyOrDeal == "deal")
                    {
                        return new JObject { { "price", 1 } }.ToString();
                    }
                    return new JObject { { "nowPrice", 1 } }.ToString();
                case SortFilterType.Sort_StarCount:
                case SortFilterType.Sort_StarCount_High:
                    return new JObject { { "starCount", -1 } }.ToString();
                case SortFilterType.Sort_StarCount_Low:
                    return new JObject { { "starCount", 1 } }.ToString();
            }
            return "{}";
        }

        public JArray getDexDomainSellDetail(string fullDomain)
        {
            string findStr = new JObject { { "fullDomain", fullDomain } }.ToString();
            string fieldStr = new JObject { { "fullDomain",1},{"sellType",1 }, { "ttl", 1}, {"assetName",1 }, { "nowPrice", 1 }, { "salePrice", 1 }, { "endPrice", 1 }, { "seller", 1 }, { "startTimeStamp", 1 }, { "_id", 0 } }.ToString();
            var queryRes = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, fieldStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var res = new JArray
            {
                queryRes.Select(p => {
                    var jo = (JObject)p;
                    var tmp = jo["nowPrice"].ToString();
                    jo.Remove("nowPrice");
                    jo.Add("nowPrice", NumberDecimalHelper.formatDecimal(tmp));
                    tmp = jo["salePrice"].ToString();
                    jo.Remove("salePrice");
                    jo.Add("salePrice", NumberDecimalHelper.formatDecimal(tmp));
                    tmp = jo["endPrice"].ToString();
                    jo.Remove("endPrice");
                    jo.Add("endPrice", NumberDecimalHelper.formatDecimal(tmp));
                    return jo;
                })
            };

            return res;
        }
        public JArray getDexDomainSellOther(string fullDomain, int pageNum=1, int pageSize=10)
        {
            string findStr = new JObject { { "fullDomain", fullDomain } }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };
            
            var res = queryRes.Select(p =>
            {
                return new JObject {
                    {"assetHash",p["assetHash"] },
                    {"assetName",p["assetName"] },
                    {"address",p["buyer"] },
                    {"price",NumberDecimalHelper.formatDecimal(p["price"].ToString()) },
                    {"time", p["time"] },
                    {"orderType", MarketType.Buy },
                    {"sellType", -1 },
                };
            }).ToArray();

            var count = res.Count();
            
            return new JArray { new JObject {
                {"count", res.Count()},
                { "list", new JArray { res.Skip((pageNum - 1) * pageSize).Take(pageSize) } }
            } };
        }
        public JArray getDexDomainBuyDetail(string fullDomain, string buyer)
        {
            string findStr = new JObject { { "fullDomain", fullDomain },{ "buyer", buyer} }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var info = queryRes[0];
            var res = new JArray
            {
                new JObject{
                    { "fullDomain",  fullDomain},
                    { "buyer", buyer},
                    { "assetName", info["assetName"].ToString()},
                    { "price", NumberDecimalHelper.formatDecimal(info["price"].ToString())},
                    { "time", info["time"]},
                    { "ttl", info["ttl"]},
                }
            };


            return res;
        }
        public JArray getDexDomainBuyOther(string fullDomain, string buyer, int pageNum=1, int pageSize=10)
        {
            var count = 0;
            List<JObject> list = new List<JObject>();
            // sellinfo
            string findStr = new JObject { { "fullDomain", fullDomain } }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, findStr);
            if (queryRes != null && queryRes.Count > 0)
            {
                pageSize -= 1;
                count += 1;
                list.Add(new JObject
                {
                    {"assetHash", queryRes[0]["assetHash"] },
                    {"assetName", queryRes[0]["assetName"] },
                    {"address", queryRes[0]["seller"] },
                    {"price", NumberDecimalHelper.formatDecimal(queryRes[0]["startPrice"].ToString()) },
                    {"time", queryRes[0]["sellTime"] },
                    {"orderType", MarketType.Sell },
                    {"sellType", queryRes[0]["sellType"] }
                });
            }
            // buyinfo
            findStr = new JObject { { "fullDomain", fullDomain },{ "buyer", new JObject { {"$ne", buyer } } } }.ToString();
            queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr, "{}", pageSize*(pageNum-1), pageSize);
            if (queryRes != null && queryRes.Count > 0)
            {
                var res = queryRes.Select(p =>
                {
                    var jo = (JObject)p;
                    var address = jo["buyer"].ToString();
                    jo.Remove("buyer");
                    jo.Add("address", address);
                    var price = NumberDecimalHelper.formatDecimal(jo["price"].ToString());
                    jo.Remove("price");
                    jo.Add("price", price);
                    jo.Add("orderType", MarketType.Buy);
                    jo.Add("sellType", -1);
                    return jo;
                }).ToArray();
                if (res != null && res.Count() > 0) list.AddRange(res);
            }

            if (list.Count() == 0) return new JArray { };

            return new JArray
            {
                new JObject{
                    { "count", count},
                    { "list", new JArray{ list } }
                }
            };
        }

        public JArray getDexDomainOrderDeal(string address, int pageNum=1, int pageSize=10)
        {
            // 成交
            var findStr = new JObject { { "$or", new JArray { new JObject { { "seller", address } }, new JObject { { "buyer", address } } } } }.ToString();
            var dealCount = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainDealHistStateCol, findStr);
            if (dealCount == 0) return new JArray { };

            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainDealHistStateCol, findStr, "{}", pageSize * (pageNum - 1), pageSize);
            var res = queryRes.Select(p =>
            {
                var jo = new JObject();
                //jo.Add("orderType", MarketType.Deal);
                jo.Add("orderType", p["displayName"].ToString() == "NNSsell" ? MarketType.Sell:MarketType.Buy);
                jo.Add("sellType", -1);
                jo.Add("fullDomain", p["fullDomain"]);
                jo.Add("nowPrice", NumberDecimalHelper.formatDecimal(p["price"].ToString()));
                jo.Add("saleRate", "0");
                jo.Add("assetName", p["assetName"]);
                jo.Add("isDeal", true);
                return jo;
            });

            return new JArray { new JObject {
                { "count", dealCount },
                { "list", new JArray{res } }
            } };
        }
        public JArray getDexDomainOrder(string address, string dealType/*0/1, 0表示未成交，1表示已成交*/, int pageNum=1, int pageSize=10)
        {
            if(dealType == "1")
            {
                return getDexDomainOrderDeal(address, pageNum, pageSize);
            }
            // sellType + fullDomain + nowPrice + assetName + saleRate + isDeal
            // 出售
            string findStr = new JObject { { "seller", address} }.ToString();
            var sellCount = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, findStr);

            List<JObject> list = new List<JObject>();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, findStr, "{}", pageSize*(pageNum-1), pageSize);
            if(queryRes != null && queryRes.Count > 0)
            {
                var res = queryRes.Select(p =>
                {
                    var jo = new JObject();
                    jo.Add("orderType", MarketType.Sell);
                    jo.Add("sellType", p["sellType"]);
                    jo.Add("fullDomain", p["fullDomain"]);
                    jo.Add("nowPrice", NumberDecimalHelper.formatDecimal(p["nowPrice"].ToString()));
                    jo.Add("saleRate", NumberDecimalHelper.formatDecimal(p["saleRate"].ToString()));
                    jo.Add("assetName", p["assetName"].ToString());
                    jo.Add("isDeal", false);
                    return jo;
                }).ToArray();
                list.AddRange(res);
            }
            
            // 求购
            findStr = new JObject { { "buyer", address } }.ToString();
            var buyCount = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr);
            if (sellCount + buyCount == 0) return new JArray { };
            if (pageSize == list.Count() || buyCount == 0) 
            {
                return new JArray { new JObject {
                    { "count", sellCount+buyCount },
                    { "list", new JArray{ list } }
                } };
            }

            var newPageNum = pageNum - (int)sellCount / pageSize;
            var newLimit = pageSize -= list.Count();
            queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr, "{}", (newPageNum-1)*pageSize, newLimit);
            if (queryRes != null && queryRes.Count > 0)
            {
                var res = queryRes.Select(p =>
                {
                    var jo = new JObject();
                    jo.Add("orderType", MarketType.Buy);
                    jo.Add("sellType", -1);
                    jo.Add("fullDomain", p["fullDomain"]);
                    jo.Add("nowPrice", NumberDecimalHelper.formatDecimal(p["price"].ToString()));
                    jo.Add("saleRate", "0");
                    jo.Add("assetName", p["assetName"].ToString());
                    jo.Add("isDeal", false);
                    return jo;
                }).ToArray();
                list.AddRange(res);
            }
            return new JArray { new JObject {
                    { "count", sellCount+buyCount },
                    { "list", new JArray{ list } }
                } };
        }

        // 查询域名持有信息
        public JArray getDexDomainInfo(string domain, string address)
        {
            // 未持有+持有(上架/未上架)
            string findStr = new JObject { { "fulldomain", domain }, { "TTL", new JObject { { "$gt", TimeHelper.GetTimeStamp() } } } }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, domainOwnerCol, findStr);
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray { new JObject { { "domain", domain }, { "isOwn", false }, { "isLaunch", false } } };
            }
            bool isOwn = queryRes[0]["owner"].ToString() == address;
            bool isLaunch = queryRes[0]["dexLaunchFlag"] != null && queryRes[0]["dexLaunchFlag"].ToString() == "1";

            return new JArray { new JObject { { "domain", domain }, { "isOwn", isOwn }, { "isLaunch", isLaunch } } };
        }
        
        // 发起挂单时，我的域名列表(去掉过期的、去掉出售中的、去掉已绑定的)
        public JArray getDexDomainCanUseList(string address, string domainPrefix="", int pageNum=1, int pageSize=10)
        {
            var findJo = new JObject { { "owner", address } };
            if (domainPrefix != "")
            {
                findJo.Add("fulldomain", MongoFieldHelper.newRegexFilter(domainPrefix));
            }
            findJo.Add("TTL", new JObject { { "$gt", TimeHelper.GetTimeStamp() } });
            findJo.Add("$or", MongoFieldHelper.newNoExistEqFilter("bindFlag", "1"));
            
            string findStr = findJo.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, domainOwnerCol, findStr);
            if (count == 0) return new JArray { };

            string fieldStr = new JObject { { "fulldomain",1},{"TTL",1 },{ "_id",0} }.ToString();
            string sortStr = new JObject { { "fulldomain", 1} }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, domainOwnerCol, findStr, sortStr, pageSize*(pageNum-1), pageSize, fieldStr);
            
            return new JArray { new JObject {
                {"count",count },
                {"list", queryRes}
            } };
        }

        // 查询我的域名(排序方式:字母顺序/到期时间, 筛选：全部/出售中/未出售/已映射/未映射)
        public JArray getDexDomainList(string address, string domainPrefix="", string queryFilter="", string sortFilter= "", int pageNum=1, int pageSize=10)
        {
            // fulldomain + ttl + data + isUsing + isSelling + isBind + isTTL
            var findJo = new JObject { { "owner", address } };
            if(domainPrefix != "")
            {
                findJo.Add("fulldomain", MongoFieldHelper.newRegexFilter(domainPrefix));
            }
            if(queryFilter == SortFilterType.SellFilter_Yes)
            {
                findJo.Add("dexLaunchFlag", MongoFieldHelper.newExistEqFilter("1"));
            } else if(queryFilter == SortFilterType.SellFilter_Not)
            {
                findJo.Add("$or", MongoFieldHelper.newNoExistEqFilter("1", "dexLaunchFlag"));
            } else if(queryFilter == SortFilterType.mapFilter_Yes)
            {
                findJo.Add("data", new JObject { { "$ne", ""} });
            } else if(queryFilter == SortFilterType.mapFilter_Not)
            {
                findJo.Add("data", "");
            } else if(queryFilter == SortFilterType.bindFilter_Yes)
            {
                findJo.Add("bindFlag", MongoFieldHelper.newExistEqFilter("1"));
            } else if(queryFilter == SortFilterType.bindFilter_Not)
            {
                findJo.Add("$or", MongoFieldHelper.newNoExistEqFilter("1", "bindFlag"));
            }
            findJo.Add("TTL", new JObject { { "$gt", TimeHelper.GetTimeStamp() } });

            var sortJo = new JObject();
            if(sortFilter == "" || sortFilter == SortFilterType.Sort_fullDomain)
            {
                sortJo.Add("fulldomain",1);
            } else if(sortFilter == SortFilterType.Sort_ttl)
            {
                sortJo.Add("TTL", -1);
            }
            
            var findStr = findJo.ToString();
            var sortStr = sortJo.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, domainOwnerCol, findStr, sortStr, pageSize * (pageNum - 1), pageSize);
            if (queryRes == null || queryRes.Count() == 0) return new JArray { };

            var res = queryRes.Select(p =>
            {
                var jo = new JObject();
                jo.Add("fulldomain", p["fulldomain"]);
                jo.Add("ttl", p["TTL"]);
                jo.Add("data", p["data"]);
                jo.Add("isUsing", p["data"].ToString() != "");
                jo.Add("isSelling", p["dexLaunchFlag"] != null && p["dexLaunchFlag"].ToString() == "1");
                jo.Add("isBind", p["bindFlag"] != null && p["bindFlag"].ToString() == "1");
                jo.Add("isTTL", long.Parse(p["TTL"].ToString()) < TimeHelper.GetTimeStamp());
                return jo;
            }).ToArray();

            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, domainOwnerCol, findStr);
            
            return new JArray { new JObject {
                {"count", count },
                {"list", new JArray{ res } }
            } };
        }

        // 查询订单排行
        public JArray getOrderRange(decimal nncAmount)
        {
            string findStr = new JObject { { "mortgagePayments", new JObject { { "$gte", nncAmount} } } }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, findStr);

            return new JArray { new JObject { {"orderRange", count + 1 } } };
        }

        public JArray searchDexDomainInfo(string domain)
        {
            /**
             * 可以竞拍: domain + state
             * 竞拍中  : domain + state + price
             * 未出售  : domain + state
             * 出售中  : domain + state + price
             */
            string fulldomain = domain;
            string state = null;
            string price = null;
            string findStr = new JObject { { "fulldomain", domain }, { "TTL", new JObject { { "$gt", TimeHelper.GetTimeStamp() } } } }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, domainOwnerCol, findStr);
            if(queryRes != null && queryRes.Count > 0)
            {
                fulldomain = queryRes[0]["fulldomain"].ToString();
                bool isLaunch = queryRes[0]["dexLaunchFlag"] != null && queryRes[0]["dexLaunchFlag"].ToString() == "1";
                if(isLaunch)
                {
                    state = DomainState.YesSelling;
                    price = queryRes[0]["dexLaunchPrice"].ToString();
                } else
                {
                    state = DomainState.NotSelling;
                    price = "0";
                }
            } else
            {
                var findJo = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM }, "auctionState");
                findStr = findJo.ToString();
                string fieldStr = new JObject { { "maxPrice", 1} }.ToString();
                queryRes = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateCol, fieldStr, findStr);
                if(queryRes != null && queryRes.Count > 0)
                {
                    state = DomainState.Auctioning;
                    price = NumberDecimalHelper.formatDecimal(queryRes[0]["maxPrice"].ToString());
                } else
                {
                    state = DomainState.CanAuction;
                    price = "0";
                }
            }

            return new JArray { new JObject { {"fulldomain", fulldomain },{ "state", state}, {"price", price } } };
        }

        public JArray searchDexDomainLikeInfo(string domainPrefix, int pageNum=1, int pageSize=10)
        {
            var findJo = MongoFieldHelper.likeFilter("fullDomain", domainPrefix);
            findJo.Add("ttl", new JObject { { "$gte", TimeHelper.GetTimeStamp() } });
            string findStr = findJo.ToString();
            string fieldStr = new JObject { { "fullDomain", 1 }, { "starCount", 1 }, { "assetName", 1 }, { "nowPrice", 1 }, { "saleRate", 1 }, { "sellType", 1 }, { "_id", 0 } }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, findStr);
            if (count == 0) return new JArray { };

            string sortStr = new JObject { { "fullDomain",1} }.ToString();
            var queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            var res = queryRes.Select(p =>
            {
                var jo = (JObject)p;
                var tmp = NumberDecimalHelper.formatDecimal(jo["nowPrice"].ToString());
                jo.Remove("nowPrice");
                jo.Add("nowPrice", tmp);
                tmp = NumberDecimalHelper.formatDecimal(jo["saleRate"].ToString());
                jo.Remove("saleRate");
                jo.Add("saleRate", tmp);
                
                jo.Add("isStar", false);
                jo.Remove("starCount");
                return jo;
            }).ToArray();
            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }
        
        public JArray starDexDomain(string address,  string domain, string starFlag="0"/*1表示开始关注;0表示取消关注*/)
        {
            string findStr = new JObject { { "address", address},{ "fullDomain", domain} }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexStarStateCol, findStr);
            
            
            if((queryRes == null || queryRes.Count == 0))
            {
                if(starFlag == "1")
                {
                    // 开始关注&Null
                    var info = new JObject { { "address", address }, { "fullDomain", domain }, { "state", StarState.YesStar }, { "time", TimeHelper.GetTimeStamp() } }.ToString();
                    mh.InsertOneData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexStarStateCol, info);
                }
            } else
            {
                // 开始关注&NotNull/取消关注
                string state = starFlag == "0" ? StarState.NotStar : StarState.YesStar;
                if (queryRes[0]["state"].ToString() != state)
                {
                    string updateStr = new JObject { { "$set", new JObject { { "state", state }, { "time", TimeHelper.GetTimeStamp() } } } }.ToString();
                    mh.UpdateData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexStarStateCol, updateStr, findStr);
                }
            }
            return new JArray { new JObject { { "res", true }} };
        }
    }

    class StarState
    {
        public static string YesStar = "1";
        public static string NotStar = "0";
    }

    class DomainState
    {
        public static string CanAuction = "CanAuction"; // 可竞拍
        public static string Auctioning = "Auctioning"; // 竞拍中
        public static string NotSelling = "NotSelling"; // 未出售
        public static string YesSelling = "YesSelling"; // 出售中
    }
    class MarketType
    {
        public static string Sell = "Selling";
        public static string Buy = "Buying";
        public static string Deal = "Dealing";
    }
   

    class SortFilterType
    {
        // 排序
        public const string Sort_MortgagePayments = "MortgagePayments";
        public const string Sort_MortgagePayments_High = "MortgagePayments_High";
        public const string Sort_MortgagePayments_Low = "MortgagePayments_Low";
        public const string Sort_LaunchTime = "LaunchTime";// "LaunchTime"; // 默认上架最新
        public const string Sort_LaunchTime_New = "LaunchTime_New";
        public const string Sort_LaunchTime_Old = "LaunchTime_Old";
        public const string Sort_Price = "Price"; //
        public const string Sort_Price_High = "Price_High";
        public const string Sort_Price_Low = "Price_Low";
        public const string Sort_StarCount = "StarCount"; // "Star"; //默认关注最多
        public const string Sort_StarCount_High = "StarCount_High";
        public const string Sort_StarCount_Low = "StarCount_Low";

        public const string Sort_fullDomain = "fulldomain";
        public const string Sort_ttl = "ttl";
        // 资产晒选
        public static string AssetFilter_All = "ALL";
        public static string AssetFilter_CGAS = "CGAS";
        public static string AssetFilter_NNC = "NNC";
        // 关注晒选
        public static string StarFilter_All = "ALL";
        public static string StarFilter_Mine = "Mine";
        public static string StarFilter_Other = "Other";
        // 出售筛选
        public static string SellFilter_All = "ALL";
        public static string SellFilter_Yes = "Sell";
        public static string SellFilter_Not = "NotSell";
        // 绑定筛选
        public static string bindFilter_All = "ALL";
        public static string bindFilter_Yes = "Bind";
        public static string bindFilter_Not = "NotBind";
        // 映射筛选
        public static string mapFilter_All = "ALL";
        public static string mapFilter_Yes = "Map";
        public static string mapFilter_Not = "NotMap";

    }
}
