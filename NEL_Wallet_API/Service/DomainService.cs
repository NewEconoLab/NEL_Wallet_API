﻿using NEL_Wallet_API.Controllers;
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
        public string domainOwnerCol { get; set; }


        public JArray getDomainByAddress(string owner, string root = ".test")
        {
            string parenthash = DomainHelper.nameHash(root.Substring(1)).ToString();
            JObject queryFilter = new JObject() { { "owner", owner },{ "parenthash", parenthash } };
            JObject queryField = MongoFieldHelper.toReturn(new string[] { "domain", "resolver" , "TTL", "data" }) ;
            JArray queryRes = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, domainOwnerCol, queryField.ToString(), queryFilter.ToString());
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }
            return new JArray() {queryRes.Select(p => {
                JObject jo = (JObject)p;
                string resolverAddr = jo["data"].ToString();
                jo.Remove("data");
                jo.Add("resolverAddr", resolverAddr);
                string ttl = jo["TTL"].ToString();
                jo.Remove("TTL");
                jo.Add("ttl", ttl);
                string domain = jo["domain"].ToString();
                jo.Remove("domain");
                jo.Add("domain",domain+root);
                
                return jo;
            }).ToArray() };
        }

        public JArray getResolvedAddress(string fulldomain)
        {
            int split = fulldomain.IndexOf(".");
            string domain = fulldomain.Substring(0, split);
            string root = fulldomain.Substring(split+1);
            string parenthash = DomainHelper.nameHash(root).ToString();
            string findstr = new JObject() { { "domain", domain}, { "parenthash", parenthash }, { "protocol", "addr" } }.ToString();
            string fieldstr = MongoFieldHelper.toReturn(new string[] {"TTL", "data" }).ToString();
            JArray queryRes = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, domainOwnerCol, fieldstr, findstr);
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }
            return queryRes;
        }
    }
}
