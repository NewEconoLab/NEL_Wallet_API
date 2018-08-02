using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class DomainService
    {
        public mongoHelper mh { get; set; }
        public string notify_mongodbConnStr { get; set; }
        public string notify_mongodbDatabase { get; set; }
        public string queryDomainCollection { get; set; }
        public string domainResolver { get; set; }


        public JArray getDomainByAddressNew(string owner, string root = ".test")
        {
            string parenthash = DomainHelper.nameHash(root.Substring(1)).ToString();
            JObject queryFilter = new JObject() { { "owner", owner },{ "parenthash", parenthash } };
            JObject queryField = MongoFieldHelper.toReturn(new string[] { "domain", "resolver" , "TTL", "data" }) ;
            JArray queryRes = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, "domainOwnerCol", queryField.ToString(), queryFilter.ToString());
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }
            return new JArray() {queryRes.Select(p => {
                JObject jo = (JObject)p;
                string resolverAddr = jo["data"].ToString();
                jo.Add("resolverAddr", resolverAddr);
                jo.Remove("data");
                return jo;
            }).ToArray() };
        }
        public JArray getDomainByAddress(string owner, string root = ".test")
        {
            string parenthash = DomainHelper.nameHash(root.StartsWith(".") ? root.Substring(1) : root).ToString();
            JObject queryFilter = new JObject();
            queryFilter.Add("owner", owner);
            queryFilter.Add("parenthash", parenthash);
            JArray queryRes = mh.GetData(notify_mongodbConnStr, notify_mongodbDatabase, queryDomainCollection, queryFilter.ToString());
            if(queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }
            
            return new JArray(){
                queryRes.Select(item => {

                    // 全域名
                    string fullDomain = null;   // 域名
                    string resolver = null;     // 解析器Hex
                    string resolverAddr = null;     // 解析器地址
                    string ttl = null;      // 到期时间

                    // 本域名
                    string slfDomain = Convert.ToString(item["domain"]);
                    fullDomain += slfDomain;
                    
                    // 父域名
                    string parentDomain = root;
                    fullDomain += parentDomain;
                    
                    // 解析器、解析地址、过期时间
                    resolver = Convert.ToString(item["resolver"]);
                    resolverAddr = "";
                    {
                        var test = DomainHelper.nameHash(parentDomain.Substring(1));
                        var a_test = DomainHelper.nameHashSub(test, slfDomain);

                        // 获取解析器映射地址
                        JObject resolverFilter = new JObject();
                        resolverFilter.Add("namehash", a_test.ToString());
                        resolverFilter.Add("protocol", "addr");
                        JObject resolverSort = new JObject();
                        resolverSort.Add("blockindex", -1);
                        JArray resolverRes = mh.GetData(notify_mongodbConnStr, notify_mongodbDatabase, domainResolver, resolverFilter.ToString());
                        if (resolverRes != null && resolverRes.Count >= 1)
                        {
                            JToken last = resolverRes.OrderByDescending(ppp => Convert.ToString(ppp["getTime"])).First();
                            resolverAddr = Convert.ToString(last["data"]);
                        }
                    }
                    ttl = Convert.ToString(item["TTL"]);
                    string getTime = Convert.ToString(item["getTime"]);
                    return new JObject { { "domain", fullDomain }, { "resolver", resolver }, { "resolverAddress", resolverAddr }, { "ttl", ttl }
                        , { "gettime", getTime }, { "slfDomain", slfDomain} };

                }).GroupBy(ii => ii["domain"], (kkk, ggg) => {
                    return ggg.OrderByDescending(iii => iii["gettime"]).ToArray().First();

                }).Where(pItem => {
                    // filter: 过滤掉过期的且被他人再次使用的
                    string slfDomain = Convert.ToString(pItem["slfDomain"]);
                    long expire = long.Parse(Convert.ToString(pItem["ttl"]));
                    long nowtime = TimeHelper.GetTimeStamp();
                    if (expire != 0 && expire < nowtime)
                    {
                        string filter = "{$and: [{\"domain\":\"" + slfDomain + "\"}" + "," + "{\"parenthash\":\"" + parenthash + "\"}" + "," + "{\"owner\":\"" + "{$not:" + owner + "}" + "\"}" + "]}";
                        JArray queryDomainResFilter = mh.GetData(notify_mongodbConnStr, notify_mongodbDatabase, queryDomainCollection, filter);
                        if (queryDomainResFilter != null && queryDomainResFilter.Count() > 0) return false;
                    }
                    return true;

                }).Select(ppItem => {
                    ppItem.Remove("gettime");
                    ppItem.Remove("slfDomain");
                    return ppItem;

                }).ToArray() };
        }

    }
}
