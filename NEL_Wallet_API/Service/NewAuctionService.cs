﻿using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class NewAuctionService
    {
        public mongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }
        public string auctionStateCol { get; set; }
        public string cgasBalanceStateCol { get; set; }
        public string domainStateCol { get; set; }

        public JArray getdomainAuctionInfo(string domain)
        {
            domain = domain.ToLower();
            string findstr = new JObject() { { "fulldomain", domain } }.ToString();
            string sortstr = new JObject() { { "startTime.blockindex", -1 } }.ToString();
            //string fieldstr = new JObject() { { "auctionState",1} }.ToString();
            //JArray res = mh.GetDataPagesWithField(mongodbConnStr, mongodbDatabase, auctionStateCol, fieldstr, 1, 1, sortstr, findstr);
            JArray res = mh.GetDataPages(mongodbConnStr, mongodbDatabase, auctionStateCol, sortstr, 1, 1, findstr);
            if (res == null || res.Count() == 0)
            {
                return new JArray();
            }

            return new JArray() { res.Select(p => {
                  JObject jo = (JObject)p;
                string maxPrice = NumberDecimalHelper.formatDecimal(p["maxPrice"].ToString());
                jo.Remove("maxPrice");
                jo.Add("maxPrice", maxPrice);
                return jo;
            }).ToArray()[0] };
        }
        // 移动端调用：获取注册器竞拍账户余额
        public JArray getRegisterAddressBalance(string address, string registerhash)
        {
            registerhash = registerhash.StartsWith("0x") ? registerhash: "0x" + registerhash;
            string findstr = new JObject() { { "address", address }, { "register", registerhash } }.ToString();
            string fieldstr = new JObject() { {"balance",1 } }.ToString();
            JArray res = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, cgasBalanceStateCol, fieldstr, findstr);
            if (res == null || res.Count() == 0)
            {
                return new JArray();
            }
            JObject jo = (JObject)res[0];
            string value = jo["balance"].ToString();
            value = NumberDecimalHelper.formatDecimal(value);
            jo.Remove("balance");
            jo.Add("balance", value);
            return new JArray() { jo };
        }
        // 移动端调用：获取竞拍状态
        public JArray getAuctionState(string domain)
        {
            domain = domain.ToLower();
            string findstr = new JObject() { { "fulldomain", domain } }.ToString();
            string sortstr = new JObject() { { "startTime.blockindex", -1} }.ToString();
            JArray res = mh.GetDataPages(mongodbConnStr, mongodbDatabase, auctionStateCol, sortstr, 1,1, findstr);
            if(res == null || res.Count() ==0)
            {
                return new JArray();
            }
            return new JArray() { res[0] };
        }
        // 移动端调用：获取域名信息
        public JArray getDomainInfo(string domain)
        {
            domain = domain.ToLower();
            string domainSub = null;
            string parenthash = null;
            if(domain.StartsWith(".") || !domain.Contains("."))
            {
                // 根域名查询
                domainSub = domain.StartsWith(".") ? domain.Substring(1) : domain;
                parenthash = "";
            } else
            {
                // 二级域名查询
                int split = domain.LastIndexOf(".");
                domainSub = domain.Substring(0, split);
                parenthash = DomainHelper.nameHash(domain.Substring(split + 1)).ToString();
            }
            
            string findstr = new JObject() { { "domain", domainSub }, { "parenthash", parenthash } }.ToString();
            string fieldstr = MongoFieldHelper.toReturn(new string[] { "owner","register","resolver","TTL","parentOwner","root"}).ToString();
            JArray res = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, domainStateCol, fieldstr, findstr);
            if(res == null || res.Count() ==0)
            {
                return new JArray();
            }
            return new JArray() { res[0] };
        }
        public JArray getAcutionInfoCount(string address, string root=".neo")
        {
            root = root.ToLower();
            JObject stateFilter = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_START, AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM, AuctionState.STATE_END }, "auctionState");
            JObject addressFilter = new JObject() { { "$or", new JArray() { new JObject() { { "addwholist.address", address } }, new JObject() { { "startAddress", address } }, new JObject() { { "endAddress", address } } } } };
            JObject rootFilter = MongoFieldHelper.likeFilter("fulldomain", root);

            string findStr = new JObject() { { "$and", new JArray() { stateFilter, addressFilter, rootFilter } } }.ToString();
            long count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, auctionStateCol, findStr);

            return new JArray() { new JObject() { { "count", count } } };
        }
        public JArray getAuctionInfoByAddress(string address, int pageNum = 1, int pageSize = 10, string root=".neo")
        {
            root = root.ToLower();
            JObject stateFilter = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_START, AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM, AuctionState.STATE_END }, "auctionState");
            JObject addressFilter = new JObject() { {"$or", new JArray() { new JObject() { { "addwholist.address", address } }, new JObject() { { "startAddress", address } }, new JObject() { { "endAddress", address } } } } };
            //JObject rootFilter = MongoFieldHelper.likeFilter("fulldomain", root);
            string parenthash = DomainHelper.nameHash(root.Substring(1)).ToString();
            JObject rootFilter = new JObject() { {"parenthash", parenthash } };
            string findStr = new JObject() { { "$and", new JArray() { stateFilter, addressFilter, rootFilter } } }.ToString();
            string sortStr = new JObject() { { "startTime.blockindex", -1} }.ToString();
            //JArray res = mh.GetDataPages(mongodbConnStr, mongodbDatabase, auctionStateCol, sortStr, pageSize, pageNum, findStr);
            string fieldStr = new JObject() { { "addwholist.addpricelist", 0 } }.ToString();
            JArray res = mh.GetDataPagesWithField(mongodbConnStr, mongodbDatabase, auctionStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            if(res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            long count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, auctionStateCol, findStr);
            return new JArray() { new JObject() { {"count",count },{ "list", formatRes(res) } } };
        }

        public JArray getAuctionInfoByAuctionId(JArray auctionIdsJA, string address = "")
        {
            string[] auctionIdArr = auctionIdsJA.Select(p => p.ToString().StartsWith("0x") ? p.ToString() : "0x" + p.ToString()).ToArray();
            return getAuctionInfoByAuctionId(auctionIdArr, address);
        }
        private JArray getAuctionInfoByAuctionId(string[] auctionIdArr, string address = "")
        {
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
           
            //JArray res = mh.GetData(mongodbConnStr, mongodbDatabase, auctionStateCol, findStr);
            JArray res = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, auctionStateCol, new JObject() { { "addwholist.addpricelist", 0 } }.ToString(), findStr);
            if (res == null || res.Count == 0)
            {
                return new JArray() { };
            }

            if (address == "")
            {
                return new JArray() { new JObject() { { "count", res.Count }, { "list", formatRes(res) } } };
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

            return new JArray() { new JObject() { { "count", res.Count }, { "list", formatRes(res) } } };
        }

        private JArray formatRes(JArray res)
        {
            return new JArray()
            {
                res.Select(p => {
                    JObject jo = (JObject)p;
                    string joStr = jo.ToString();
                    if(joStr.Contains("maxPrice"))
                    {
                        string value = jo["maxPrice"].ToString();
                        value = NumberDecimalHelper.formatDecimal(value);
                        jo.Remove("maxPrice");
                        jo.Add("maxPrice", value);
                    }
                    if(joStr.Contains("addwholist") && joStr.Contains("totalValue"))
                    {
                        JArray ja = JArray.Parse(jo["addwholist"].ToString());
                        ja = new JArray()
                        {
                            ja.Select(pb => {
                                JObject job = (JObject)pb;
                                string vb = job["totalValue"].ToString();
                                vb = NumberDecimalHelper.formatDecimal(vb);
                                job.Remove("totalValue");
                                job.Add("totalValue", vb);

                                vb = job["curTotalValue"].ToString();
                                vb = NumberDecimalHelper.formatDecimal(vb);
                                job.Remove("curTotalValue");
                                job.Add("curTotalValue", vb);
                                return job;
                            }).ToArray()
                        };
                        jo.Remove("addwholist");
                        jo.Add("addwholist", ja);
                    }
                    return jo;
                }).ToArray()
            };
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
        public const string STATE_NoUsed = "0701";
    }
}
