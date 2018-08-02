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
        public string queryDomainCollection { get; set; }
        public string domainResolver { get; set; }


        public JArray getDomainByAddress(string owner, string root = ".test")
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
    }
}
