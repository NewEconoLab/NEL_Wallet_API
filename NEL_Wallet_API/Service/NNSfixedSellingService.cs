﻿using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEL_Wallet_API.Service
{
    public class NNSfixedSellingService
    {
        public mongoHelper mh { get; set; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }

        public string NNSfixedSellingColl { get; set; } = "0x7a64879a21b80e96a8bc91e0f07adc49b8f3521e";
        public string domainCenterColl { get; set; } = "0xbd3fa97e2bc841292c1e77f9a97a1393d5208b48";
        
        public JArray getDomainSellingListByAddress(string address, string root, string sellorbuy = "sell", int pageNum=1, int pageSize=10)
        {
            root = root.ToLower();
            root = root.StartsWith(".") ? root : "." + root;
            var findJo = newOrFilter("fullDomain", "\\" + root);
            findJo.Add("displayName", "NNSfixedSellingBuy");

            if (sellorbuy == "sell")
            {
                findJo.Add("seller", address);
            } else if(sellorbuy == "buy")
            {
                findJo.Add("addr", address);
            } else
            {
                findJo.Add("$or", new JArray { new JObject() { { "seller", address } }, new JObject() { { "addr", address } } });
            }
            string findStr = findJo.ToString();
            long cnt = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, findStr);
            if (cnt == 0) return new JArray { };

            string fieldStr = MongoFieldHelper.toReturn(new string[] {"blockindex", "fullDomain", "price" }).ToString();
            string sortStr = new JObject() { {"blockindex",-1 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, pageSize, pageNum, sortStr, findStr);


            var indexs = query.Select(p => (long)p["blockindex"]).Distinct().ToArray();
            var indexDict = getBlocktime(indexs);

            var res = new JArray
            {
                query.Select(p => {
                    JObject jo = (JObject)p;
                    jo.Add("time", indexDict.GetValueOrDefault((long)jo["blockindex"]));
                    jo.Remove("blockindex");
                    return jo;
                }).ToArray()
            };
            
            return new JArray { new JObject() {
                {"count", cnt },
                {"list", res }
            } };
        }
        public JArray getNNCfromSellingHash(string address)
        {
            string findStr = new JObject() { {"address", address },{"register", NNSfixedSellingColl } }.ToString();
            string fieldStr = new JObject() { {"address",1 }, { "balance", 1 } }.ToString();
            var query = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, "nnsFixedSellingBalanceState", fieldStr, findStr);
            if (query == null || query.Count == 0) return new JArray { };


            return new JArray { new JObject() {
                {"address", query[0]["address"] },
                {"balance", NumberDecimalHelper.formatDecimal(query[0]["balance"].ToString())}
            } };
        }
        public bool hasNNfixedSelling(string domain, long blockindex, out string owner, out string price)
        {
            string findStr = new JObject() { {"fullDomain", domain.ToLower() },{ "blockindex", new JObject() { {"$gte", blockindex } } } }.ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            string fieldStr = new JObject() { { "state", 0 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, 1, 1, sortStr, findStr);
            if(query != null && query.Count > 0)
            {
                string displayName = query[0]["displayName"].ToString();
                if(displayName == "NNSfixedSellingLaunched")
                {
                    owner = query[0]["seller"].ToString();
                    price = query[0]["price"].ToString();
                    return true;
                }
            }
            owner = "";
            price = "0";
            return false;
        }
        public JArray getNNSfixedSellingInfo(string domain)
        {
            domain = domain.ToLower();
            // domain + ttl + price + time
            string namehash = DomainHelper.nameHashFullDomain(domain);
            string findStr = new JObject() { {"namehash", namehash } }.ToString();
            string fieldStr = new JObject() { { "owner",1},{ "TTL",1} }.ToString();
            string sortStr = new JObject() { {"blockindex", -1 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainCenterColl, fieldStr, 1, 1, sortStr, findStr);
            if (query == null || query.Count == 0) return new JArray { };

            string owner = query[0]["owner"].ToString();
            string ttl = query[0]["TTL"].ToString();

            findStr = new JObject() { {"fullDomain", domain } }.ToString();
            fieldStr = new JObject() { {"price",1 }, { "displayName", 1 },{ "seller",1 },{ "blockindex",1} }.ToString();
            sortStr = new JObject() { {"blockindex", -1 } }.ToString();
            query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, 1, 1, sortStr, findStr);

            string price = "0";
            string state = "";
            if(query != null && query.Count > 0)
            {
                if (query[0]["displayName"].ToString() == "NNSfixedSellingLaunched" && !hasExpire(namehash, long.Parse(query[0]["blockindex"].ToString())))
                {
                    price = query[0]["price"].ToString();
                    state = "0901";
                    owner = query[0]["seller"].ToString() ;
                }
            }

            return new JArray
            {
                new JObject() {
                    {"domain", domain },
                    {"owner", owner },
                    {"ttl", ttl },
                    {"price", price },
                    {"state", state },
                }
            };
        }
        private bool hasExpire(string namehash, long blockindex)
        {
            var findStr = new JObject { { "namehash", namehash }, { "blockindex", new JObject { { "$lt", blockindex } } } }.ToString();
            var fieldStr = new JObject { { "TTL",1 } }.ToString();
            var sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            var queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainCenterColl, fieldStr, 1, 1, sortStr, findStr);
            if (queryRes.Count == 0 || long.Parse(queryRes[0]["TTL"].ToString()) <= TimeHelper.GetTimeStamp()) return true;
            return false;
        }
        public bool hasExpire(string namehash, string displayName= "NNSfixedSellingLaunched")
        {
            var findStr = new JObject { { "fullHash", namehash }, { "displayName", displayName } }.ToString();
            var fieldStr = new JObject { { "blockindex", 1 } }.ToString();
            var sortStr = new JObject { { "blockindex", -1 } }.ToString();
            var queryRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, 1, 1, sortStr, findStr);
            if (queryRes.Count == 0) return false;
            var index = long.Parse(queryRes[0]["blockindex"].ToString());
            return hasExpire(namehash, index);
        }

        public JArray getHasBuyListByAddress(string address, string root, int pageNum=1, int pageSize=10)
        {
            
            root = root.StartsWith(".") ? root : "."+root;
            var findJo = newOrFilter("fullDomain", "\\"+root);
            findJo.Add("seller", address);
            findJo.Add("displayName", "NNSfixedSellingBuy");
            string findStr = findJo.ToString();
            //string findStr = new JObject() { { "seller", address },{ "displayName", "NNSfixedSellingBuy" } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fullDomain", "blockindex", "price" }).ToString();
            string sortStr = new JObject() { {"blockindex",-1 } }.ToString();
            // count
            long count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, findStr);
            if(count == 0) return new JArray { };
            // list
            JObject[] res = new JObject[0];
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, pageSize, pageNum, sortStr,  findStr);
            if (query != null && query.Count > 0)
            {
                var blockindexs = query.Select(p => long.Parse(p["blockindex"].ToString())).Distinct().ToArray();
                var blockindexDict = getBlocktime(blockindexs);

                res = query.Select(p =>
                {
                    JObject jo = (JObject)p;
                    jo.Add("blocktime", blockindexDict.GetValueOrDefault(long.Parse(p["blockindex"].ToString())));
                    return jo;
                }).ToArray();
            }

            return new JArray() { new JObject() { { "count", count}, { "list", new JArray { res } } } };
        }

        private JObject newOrFilter(string key, string regex)
        {
            JObject obj = new JObject();
            JObject subobj = new JObject();
            subobj.Add("$regex", regex);
            subobj.Add("$options", "i");
            obj.Add(key, subobj);
            return obj;
        }

        private Dictionary<long, long> getBlocktime(long[] indexs)
        {
            if (indexs == null &&  indexs.Length == 0) return null;

            string findStr = MongoFieldHelper.toFilter(indexs, "index").ToString() ;
            string fieldStr = new JObject() { {"index", 1 }, { "time", 1 } }.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
            return query.ToDictionary(k => long.Parse(k["index"].ToString()), v => long.Parse(v["time"].ToString()));
        }
    }
}
