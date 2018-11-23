using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public string Bonus_mongodbConnStr { set; get; }
        public string Bonus_mongodbDatabase { set; get; }
        public string CurrentBonusCol { set; get; }
        public string BonusCol { set; get; }

        public JArray getBonusHistByAddress(string address, int pageNum = 1, int pageSize = 10)
        {
            List<String> list = mh.listCollection(Bonus_mongodbConnStr, Bonus_mongodbDatabase);
            if (list == null && list.Count == 0)
            {
                return new JArray() { };
            }
            string findstr = new JObject() { { "addr", address } }.ToString();
            JToken[] res = list.Where(p => p.StartsWith("Snapshot_NNC_") && !p.Contains("_test")).Select(p =>
            {
                string coll = p;
                JArray addrbonus = mh.GetData(Bonus_mongodbConnStr, Bonus_mongodbDatabase, coll, findstr);
                if (addrbonus == null || addrbonus.Count == 0)
                {
                    return null;
                }
                JObject bonus = (JObject)addrbonus[0];
                //if(bonus["txid"] == null || bonus["txid"].ToString() == "")
                //{
                //    return null;
                //}
                return new JObject() {
                            {"address", bonus["addr"] },
                            {"balance", NumberDecimalHelper.formatDecimal(bonus["balance"].ToString()) },
                            {"addrBonus", NumberDecimalHelper.formatDecimal(bonus["send"].ToString()) },
                            {"height", bonus["height"] },
                };
            }).Where(p => p != null).ToArray();
            if(res == null || res.Count() ==0)
            {
                return new JArray() { };
            }


            // 分红总量快照
            long[] heightArr = res.Select(p => long.Parse(p["height"].ToString())).Distinct().ToArray();
            string totalBonusFindstr = MongoFieldHelper.toFilter(heightArr, "height").ToString();
            JArray totalBonusRes = mh.GetData(Bonus_mongodbConnStr, Bonus_mongodbDatabase, "TotalSnapShot", totalBonusFindstr);
            Dictionary<long, string> totalBonusDict = null;
            if(totalBonusRes != null && totalBonusRes.Count > 0)
            {
                totalBonusDict = totalBonusRes.ToDictionary(k => long.Parse(k["height"].ToString()), v => NumberDecimalHelper.formatDecimal(v["totalValue"].ToString()));
            }

            // 区块时间
            string blocktimeFindstr = MongoFieldHelper.toFilter(heightArr, "index").ToString();
            string blocktimeFieldstr = new JObject() { {"index",1 }, { "time", 1 } }.ToString();
            JArray blocktimeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", blocktimeFieldstr, blocktimeFindstr);
            Dictionary<long, long> blocktimeDict = null;
            if (blocktimeRes != null && blocktimeRes.Count > 0)
            {
                blocktimeDict = blocktimeRes.ToDictionary(k => long.Parse(k["index"].ToString()), v => long.Parse(v["time"].ToString()));
            }
            res = res.Select(p =>
            {
                long height = long.Parse(p["height"].ToString());
                if(totalBonusDict != null && totalBonusDict.ContainsKey(height))
                {
                    p["totalValue"] = totalBonusDict.GetValueOrDefault(height);
                } else
                {
                    p["totalValue"] = 0;
                }

                if(blocktimeDict != null && blocktimeDict.ContainsKey(height))
                {
                    p["blocktime"] = blocktimeDict.GetValueOrDefault(height);
                } else
                {
                    p["blocktime"] = 0;
                }
                JObject jo = (JObject)p;
                jo.Remove("height");
                return jo;
            }).OrderByDescending(p => long.Parse(p["blocktime"].ToString())).ToArray();

            return new JArray()
            {
                new JObject() {{"count", res.Count()}, { "list",new JArray() { res.Skip(pageSize*(pageNum-1)).Take(pageSize) } } }
            };

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

        //申请分红
        public JArray applyBonus(string address)
        {
            //获取最新的分红数据表
            string curConn = mh.GetData(Bonus_mongodbConnStr, Bonus_mongodbDatabase,CurrentBonusCol,"{}")[0]["CurrentColl"].ToString();
            //获取此次分红的信息
            JObject queryFilter = new JObject() { { "addr", address }};
            JArray jAData = mh.GetData(Bonus_mongodbConnStr, Bonus_mongodbDatabase, curConn, queryFilter.ToString());
            if (jAData.Count == 0)
                return new JArray() { new JObject() { { "result", false } } };
            jAData[0]["applied"] = true;
            mh.ReplaceData(Bonus_mongodbConnStr, Bonus_mongodbDatabase, curConn, queryFilter.ToString(), jAData[0].ToString());
            return new JArray() { new JObject() { { "result", true } } };
        }

        //获取当前分红的信息
        public JArray getCurrentBonus(string address)
        {
            //获取最新的分红数据表
            string curConn = mh.GetData(Bonus_mongodbConnStr, Bonus_mongodbDatabase, CurrentBonusCol, "{}")[0]["CurrentColl"].ToString();
            //获取此次分红的信息
            JObject queryFilter = new JObject() { { "addr", address } };
            JArray jAData = mh.GetData(Bonus_mongodbConnStr, Bonus_mongodbDatabase, curConn, queryFilter.ToString());
            JObject jObject = (JObject)jAData[0];
            jObject["balance"] = NumberDecimalHelper.formatDecimal(jObject["balance"].ToString());
            jObject["send"] = NumberDecimalHelper.formatDecimal(jObject["send"].ToString());
            return new JArray() { jObject };
        }

        //获取某个地址的已得的分红记录
        public JArray getBonusByAddress(string address, int pageNum = 1, int pageSize = 10)
        {
            JObject queryFilter = new JObject() { { "addr", address } };
            JArray jArray =  mh.GetData(Bonus_mongodbConnStr, Bonus_mongodbDatabase, BonusCol, queryFilter.ToString());
            // 区块时间
            long[] heightArr = jArray.Select(p => long.Parse(p["height"].ToString())).Distinct().ToArray();
            string blocktimeFindstr = MongoFieldHelper.toFilter(heightArr, "index").ToString();
            string blocktimeFieldstr = new JObject() { { "index", 1 }, { "time", 1 } }.ToString();
            JArray blocktimeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", blocktimeFieldstr, blocktimeFindstr);
            Dictionary<long, long> blocktimeDict = null;
            if (blocktimeRes != null && blocktimeRes.Count > 0)
            {
                blocktimeDict = blocktimeRes.ToDictionary(k => long.Parse(k["index"].ToString()), v => long.Parse(v["time"].ToString()));
            }
            return new JArray(){ jArray.Select(p =>
            {
                long height = long.Parse(p["height"].ToString());


                if (blocktimeDict != null && blocktimeDict.ContainsKey(height))
                {
                    p["blocktime"] = blocktimeDict.GetValueOrDefault(height);
                }
                else
                {
                    p["blocktime"] = 0;
                }
                p["balance"] =  NumberDecimalHelper.formatDecimal(p["balance"].ToString());
                p["send"] =  NumberDecimalHelper.formatDecimal(p["send"].ToString());
                JObject jo = (JObject)p;
                jo.Remove("height");
                return jo;
            }).OrderByDescending(p => long.Parse(p["blocktime"].ToString())).ToArray() };
        }
    }
}
