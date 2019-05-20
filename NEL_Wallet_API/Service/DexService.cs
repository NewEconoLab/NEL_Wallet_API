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

        public JArray getDexDomainSellList(string address, int pageNum=1, int pageSize=10, string sortType= ""/*newtime(startTimeStamp).priceH(nowPrice).priceL.starCouunt.mortgate*/, string assetFilterType=""/*all/cgas/nnc*/, string starFilterType=""/*all/mine/other*/)
        {
            string findStr = getFindStr(assetFilterType, starFilterType);
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
            string fieldStr = new JObject { { "fullDomain", 1 }, { "owner", 1 }, { "starCount", 1 }, { "maxAssetName", 1 }, { "maxPrice", 1 }, { "maxTime", 1 }, { "_id", 0 } }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr);
            if (count == 0) return new JArray { };

            var queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            var res = queryRes.Select(p =>
            {
                var jo = (JObject)p;
                var tmp = NumberDecimalHelper.formatDecimal(jo["maxPrice"].ToString());
                jo.Remove("maxPrice");
                jo.Add("maxPrice", tmp);
                jo.Add("isNewly", nowTime < long.Parse(jo["maxTime"].ToString()) + newlyDataTimeRange);
                jo.Remove("maxTime");
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

        private string getFindStr(string assetFilterType, string starFilterType)
        {
            if (assetFilterType == "" && starFilterType == "") return "{}";

            assetFilterType = assetFilterType.ToUpper();
            JObject findJo = new JObject();
            if(assetFilterType == SortFilterType.AssetFilter_CGAS || assetFilterType == SortFilterType.AssetFilter_NNC)
            {
                findJo.Add("assetName", assetFilterType);
            }
            if(starFilterType == SortFilterType.StarFilter_Mine)
            {
                // TODO
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
            string fieldStr = new JObject { { "fullDomain",1},{"sellType",1 }, { "ttl", 1}, { "nowPrice", 1 }, { "saleRate", 1 }, { "seller", 1 }, { "startTimeStamp", 1 }, { "_id", 0 } }.ToString();
            var queryRes = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainSellStateCol, fieldStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var res = new JArray
            {
                queryRes.Select(p => {
                    var jo = (JObject)p;
                    var tmp = jo["nowPrice"].ToString();
                    jo.Remove("nowPrice");
                    jo.Add("nowPrice", NumberDecimalHelper.formatDecimal(tmp));
                    tmp = jo["saleRate"].ToString();
                    jo.Remove("saleRate");
                    jo.Add("saleRate", NumberDecimalHelper.formatDecimal(tmp));
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

            var res = ((JArray)queryRes[0]["buyerList"]).Select(p =>
            {
                var jo = (JObject)p;
                var address = jo["buyer"].ToString();
                jo.Remove("buyer");
                jo.Add("address", address);
                var price = NumberDecimalHelper.formatDecimal(jo["price"].ToString());
                jo.Remove("price");
                jo.Add("price", price);
                jo.Add("type", MarketType.Buy);
                return jo;
            }).ToArray();

            var count = res.Count();
            
            return new JArray { new JObject {
                {"count", res.Count()},
                { "list", new JArray { res.Skip((pageNum - 1) * pageSize).Take(pageSize) } }
            } };
        }
        public JArray getDexDomainBuyDetail(string fullDomain, string buyer)
        {
            string findStr = new JObject { { "fullDomain", fullDomain } }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var buyInfo = queryRes[0];
            var buyerInfo = ((JArray)queryRes[0]["buyerList"]).Where(p => p["buyer"].ToString() == buyer).First();
            var otherInfo = ((JArray)queryRes[0]["buyerList"]).Where(p => p["buyer"].ToString() != buyer).ToArray();


            var res = new JArray
            {
                new JObject{
                    {"fullDomain",  fullDomain},
                    { "ttl", buyInfo["ttl"]},
                    { "buyer", buyerInfo["buyer"]},
                    { "price", NumberDecimalHelper.formatDecimal(buyerInfo["price"].ToString())},
                    { "time", buyerInfo["time"]}
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
                    {"type", MarketType.Sell }
                });
            }
            // buyinfo
            findStr = new JObject { { "fullDomain", fullDomain } }.ToString();
            queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, dexDomainBuyStateCol, findStr);
            if (queryRes != null && queryRes.Count > 0)
            {
                var otherInfo = ((JArray)queryRes[0]["buyerList"]).Where(p => p["buyer"].ToString() != buyer).ToArray();
                var res = otherInfo.Skip((pageNum - 1) * pageSize).Take(pageSize).Select(p =>
                {
                    var jo = (JObject)p;
                    var address = jo["buyer"].ToString();
                    jo.Remove("buyer");
                    jo.Add("address", address);

                    var price = NumberDecimalHelper.formatDecimal(jo["price"].ToString());
                    jo.Remove("price");
                    jo.Add("price", price);
                    jo.Add("type", MarketType.Buy);
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
                jo.Add("fullDomain", p["fullDomain"]);
                jo.Add("nowPrice", NumberDecimalHelper.formatDecimal(p["price"].ToString()));
                jo.Add("saleRate", 0);
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
            findStr = new JObject { { "buyerList.buyer", address } }.ToString();
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
                    var cc = ((JArray)p["buyerList"]).Where(pq => pq["buyer"].ToString() == address).First();

                    var jo = new JObject();
                    jo.Add("orderType", MarketType.Buy);
                    jo.Add("fullDomain", p["fullDomain"]);
                    jo.Add("nowPrice", NumberDecimalHelper.formatDecimal(cc["price"].ToString()));
                    jo.Add("saleRate", 0);
                    jo.Add("assetName", cc["assetName"].ToString());
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

        public JArray getDexDomainList(string address, int pageNum=1, int pageSize=10)
        {
            // fulldomain + ttl + data + isUsing + isSelling + isBind + isTTL
            string findStr = new JObject { { "owner", address} }.ToString();
            var queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, "domainOwnerCol", findStr, "{}", pageSize * (pageNum - 1), pageSize);
            if (queryRes == null || queryRes.Count() == 0) return new JArray { };

            var res = queryRes.Select(p =>
            {
                var jo = new JObject();
                jo.Add("fulldomain", p["fulldomain"]);
                jo.Add("ttl", p["TTL"]);
                jo.Add("data", p["data"]);
                jo.Add("isUsing", p["data"].ToString() != "");
                jo.Add("isSelling", p["dexType"] != null && p["dexType"].ToString() == "");
                jo.Add("isBind", p["bindFlag"] != null && p["bindFlag"].ToString() == "1");
                jo.Add("isTTL", long.Parse(p["TTL"].ToString()) < TimeHelper.GetTimeStamp());
                return jo;
            }).ToArray();

            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, "domainOwnerCol", findStr);
            
            return new JArray { new JObject {
                {"count", count },
                {"list", new JArray{ res } }
            } };
        }
    }

    class MarketType
    {
        public static string Sell = "Selling";
        public static string Buy = "Buying";
        public static string Deal = "Dealing";
    }
    class SortType
    {
        public static string MortgagePayments = "MortgagePayments"; 
        public static string MortgagePayments_High = "MortgagePayments_High"; 
        public static string MortgagePayments_Low = "MortgagePayments_Low";
        public static string LaunchTime = "LaunchTime_New";// "LaunchTime"; // 默认上架最新
        public static string LaunchTime_New = "LaunchTime_New"; 
        public static string LaunchTime_Old = "LaunchTime_Old";
        public static string Price = "Price"; //
        public static string Price_High = "Price_High"; 
        public static string Price_Low = "Price_Low"; 
        public static string StarCount = "StarCount_High"; // "Star"; //默认关注最多
        public static string StarCount_High = "StarCount_High"; 
        public static string StarCount_Low = "StarCount_Low"; 
    }
    class FilterType
    {
        public static string Asset_All = "all";
        public static string Asset_CGAS = "cgas";
        public static string Asset_NNC = "nnc";

        public static string Star_All = "all";
        public static string Star_Mine = "mine";
        public static string Star_Other = "other";
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
        // 资产晒选
        public static string AssetFilter_All = "ALL";
        public static string AssetFilter_CGAS = "CGAS";
        public static string AssetFilter_NNC = "NNC";
        // 关注晒选
        public static string StarFilter_All = "ALL";
        public static string StarFilter_Mine = "Mine";
        public static string StarFilter_Other = "Other";
    }
}
