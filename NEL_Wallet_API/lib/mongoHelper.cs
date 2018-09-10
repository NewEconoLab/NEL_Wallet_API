﻿using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.IO;

namespace NEL_Wallet_API.lib
{
    public class mongoHelper
    {

        public string block_mongodbConnStr_testnet = string.Empty;
        public string block_mongodbDatabase_testnet = string.Empty;
        public string analy_mongodbConnStr_testnet = string.Empty;
        public string analy_mongodbDatabase_testnet = string.Empty;
        public string notify_mongodbConnStr_testnet = string.Empty;
        public string notify_mongodbDatabase_testnet = string.Empty;
        public string nelJsonRPCUrl_testnet = string.Empty;
        public string bonusConnStr_testnet = string.Empty;
        public string bonusDatabase_testnet = string.Empty;


        public string block_mongodbConnStr_mainnet = string.Empty;
        public string block_mongodbDatabase_mainnet = string.Empty;
        public string analy_mongodbConnStr_mainnet = string.Empty;
        public string analy_mongodbDatabase_mainnet = string.Empty;
        public string notify_mongodbConnStr_mainnet = string.Empty;
        public string notify_mongodbDatabase_mainnet = string.Empty;
        public string nelJsonRPCUrl_mainnet = string.Empty;
        
        public string queryDomainCollection_testnet = string.Empty;
        public string queryDomainCollection_mainnet = string.Empty;
        public string queryBidListCollection_testnet = string.Empty; 
        public string queryBidListCollection_mainnet = string.Empty;
        
        public string bonusNotifyCol_testnet = string.Empty;
        public string bonusNotifyFrom_testnet = string.Empty;
        public string bonusNotifyCol_mainnet = string.Empty;
        public string bonusNotifyFrom_mainnet = string.Empty;

        public string rechargeCollection_mainnet = string.Empty;
        public string rechargeCollection_testnet = string.Empty;

        public string domainOwnerCol_testnet = string.Empty;
        public string domainOwnerCol_mainnet = string.Empty;
        
        public string domainUserStateCol_testnet = string.Empty;
        public string domainUserStateCol_mainnet = string.Empty;
        public string domainStateCol_testnet = string.Empty;
        public string domainStateCol_mainnet = string.Empty;


        public string id_neo = string.Empty;
        public string id_gas = string.Empty;
        public string prikeywif_testnet = string.Empty;
        public string prikeywif_mainnet = string.Empty;
        public string gasClaimCol_testnet = string.Empty;
        public string gasClaimCol_mainnet = string.Empty;
        public string maxClaimAmount_testnet = string.Empty;
        public string maxClaimAmount_mainnet = string.Empty;
        public string batchSendInterval_testnet = string.Empty;
        public string batchSendInterval_mainnet = string.Empty;
        public string checkTxInterval_testnet = string.Empty;
        public string checkTxInterval_mainnet = string.Empty;
        public string checkTxCount_testnet = string.Empty;
        public string checkTxCount_mainnet = string.Empty;

        public string auctionStateCol_testnet = string.Empty;
        public string auctionStateCol_mainnet = string.Empty;

        public mongoHelper() {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()    //将配置文件的数据加载到内存中
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())   //指定配置文件所在的目录
                .AddJsonFile("mongodbsettings.json", optional: true, reloadOnChange: true)  //指定加载的配置文件
                .Build();    //编译成对象  

            block_mongodbConnStr_testnet = config["block_mongodbConnStr_testnet"];
            block_mongodbDatabase_testnet = config["block_mongodbDatabase_testnet"];
            analy_mongodbConnStr_testnet = config["analy_mongodbConnStr_testnet"];
            analy_mongodbDatabase_testnet = config["analy_mongodbDatabase_testnet"];
            notify_mongodbConnStr_testnet = config["notify_mongodbConnStr_testnet"];
            notify_mongodbDatabase_testnet = config["notify_mongodbDatabase_testnet"];
            nelJsonRPCUrl_testnet = config["nelJsonRPCUrl_testnet"];
            bonusConnStr_testnet = config["bonusConnStr_testnet"];
            bonusDatabase_testnet = config["bonusDatabase_testnet"];

            block_mongodbConnStr_mainnet = config["block_mongodbConnStr_mainnet"];
            block_mongodbDatabase_mainnet = config["block_mongodbDatabase_mainnet"];
            analy_mongodbConnStr_mainnet = config["analy_mongodbConnStr_mainnet"];
            analy_mongodbDatabase_mainnet = config["analy_mongodbDatabase_mainnet"];
            notify_mongodbConnStr_mainnet = config["notify_mongodbConnStr_mainnet"];
            notify_mongodbDatabase_mainnet = config["notify_mongodbDatabase_mainnet"];
            nelJsonRPCUrl_mainnet = config["nelJsonRPCUrl_mainnet"];

            queryDomainCollection_testnet = config["queryDomainCollection_testnet"];
            queryDomainCollection_mainnet = config["queryDomainCollection_mainnet"];

            queryBidListCollection_testnet = config["queryBidListCollection_testnet"];
            queryBidListCollection_mainnet = config["queryBidListCollection_mainnet"];
            
            bonusNotifyCol_testnet = config["bonusNotifyCol_testnet"];
            bonusNotifyFrom_testnet = config["bonusNotifyFrom_testnet"];
            bonusNotifyCol_mainnet = config["bonusNotifyCol_mainnet"];
            bonusNotifyFrom_mainnet = config["bonusNotifyFrom_mainnet"];

            rechargeCollection_mainnet = config["rechargeCollection_mainnet"];
            rechargeCollection_testnet = config["rechargeCollection_testnet"];


            domainOwnerCol_testnet = config["domainOwnerCol_testnet"];
            domainOwnerCol_mainnet = config["domainOwnerCol_mainnet"];

            domainUserStateCol_testnet = config["domainUserStateCol_testnet"];
            domainUserStateCol_mainnet = config["domainUserStateCol_mainnet"];
            domainStateCol_testnet = config["domainStateCol_testnet"];
            domainStateCol_mainnet = config["domainStateCol_mainnet"];

            id_neo = config["id_neo"];
            id_gas = config["id_gas"];
            prikeywif_testnet = config["prikeywif_testnet"];
            prikeywif_mainnet = config["prikeywif_mainnet"];
            gasClaimCol_testnet = config["gasClaimCol_testnet"];
            gasClaimCol_mainnet = config["gasClaimCol_mainnet"];
            maxClaimAmount_testnet = config["maxClaimAmount_testnet"];
            maxClaimAmount_mainnet = config["maxClaimAmount_mainnet"];
            batchSendInterval_testnet = config["batchSendInterval_testnet"];
            batchSendInterval_mainnet = config["batchSendInterval_mainnet"];
            checkTxInterval_testnet = config["checkTxInterval_testnet"];
            checkTxInterval_mainnet = config["checkTxInterval_mainnet"];
            checkTxCount_testnet = config["checkTxCount_testnet"];
            checkTxCount_mainnet = config["checkTxCount_mainnet"];


            auctionStateCol_testnet = config["auctionStateCol_testnet"];
            auctionStateCol_mainnet = config["auctionStateCol_mainnet"];

        }

        public JArray GetData(string mongodbConnStr,string mongodbDatabase, string coll, string findBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }      
        }

        public JArray GetDataPages(string mongodbConnStr, string mongodbDatabase, string coll,string sortStr, int pageSize, int pageNum, string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Sort(sortStr).Skip(pageSize * (pageNum-1)).Limit(pageSize).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataWithField(string mongodbConnStr, string mongodbDatabase, string coll, string fieldBson, string findBson = "{}")
        {
            var client = mongodbConnStr == null ? new MongoClient() : new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataPagesWithField(string mongodbConnStr, string mongodbDatabase, string coll, string fieldBson, int pageCount, int pageNum, string sortBson = "{}", string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).Sort(sortBson).Skip(pageCount * (pageNum - 1)).Limit(pageCount).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public long GetDataCount(string mongodbConnStr, string mongodbDatabase, string coll, string findBson="{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            var txCount = collection.Find(BsonDocument.Parse(findBson)).CountDocuments();

            client = null;

            return txCount;
        }
        
        public string InsertOneData(string mongodbConnStr, string mongodbDatabase, string coll,string insertBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                var query = collection.Find(BsonDocument.Parse(insertBson)).ToList();
                if (query.Count == 0)
                {
                    BsonDocument bson = BsonDocument.Parse(insertBson);
                    collection.InsertOne(bson);
                }
                client = null;
                return "suc";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }
        
        public string DeleteData(string mongodbConnStr, string mongodbDatabase, string coll, string deleteBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                var query = collection.Find(BsonDocument.Parse(deleteBson)).ToList();
                if (query.Count != 0)
                {
                    BsonDocument bson = BsonDocument.Parse(deleteBson);
                    collection.DeleteOne(bson);
                }
                client = null;
                return "suc";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        
        public string ReplaceData(string mongodbConnStr, string mongodbDatabase, string collName, string whereFliter, string replaceFliter)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(collName);
            try
            {
                List<BsonDocument> query = collection.Find(whereFliter).ToList();
                if (query.Count > 0)
                {
                    collection.ReplaceOne(BsonDocument.Parse(whereFliter), BsonDocument.Parse(replaceFliter));
                    client = null;
                    return "suc";
                }
                else
                {
                    client = null;
                    return "no data";
                }
            }
            catch (Exception e)
            {
                client = null;
                return e.ToString();
            }
        }

        public string ReplaceOrInsertData(string mongodbConnStr, string mongodbDatabase, string collName, string whereFliter, string replaceFliter)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(collName);
            try
            {
                List<BsonDocument> query = collection.Find(whereFliter).ToList();
                if (query.Count > 0)
                {
                    collection.ReplaceOne(BsonDocument.Parse(whereFliter), BsonDocument.Parse(replaceFliter));
                    client = null;
                    return "suc";
                }
                else
                {
                    BsonDocument bson = BsonDocument.Parse(replaceFliter);
                    collection.InsertOne(bson);
                    client = null;
                    return "suc";
                }
            }
            catch (Exception e)
            {
                client = null;
                return e.ToString();
            }
        }

        public void UpdateData(string mongodbConnStr, string mongodbDatabase, string coll, string Jdata, string Jcondition)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            collection.UpdateMany(BsonDocument.Parse(Jcondition), BsonDocument.Parse(Jdata));

            client = null;
        }

        public List<string> listCollection(string mongodbConnStr, string mongodbDatabase)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            List<string> list = database.ListCollectionNames().ToList();
            client = null;

            return list;
        }

    }
}
