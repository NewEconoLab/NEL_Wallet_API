using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NEL_Wallet_API.Controllers
{
    [Route("api/[controller]")]
    public class TestWebDbController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public string Get()
        {
            return "TestWebDbController.Get.Res:" + query();
        }
        
        // POST api/<controller>
        [HttpPost]
        public string Post()
        {
            return "TestWebDbController.Post.Res:" + query();
        }

        private long query()
        {
            //string mongodbConnStr = "mongodb://notifyDataStorage:NELqingmingzi1128@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/contractNotifyInfo?replicaSet=mgset-4977005";

            string mongodbConnStr = "mongodb://notifyDataStorage:NELqingmingzi1128@dds-bp1b36419665fdd41.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42.mongodb.rds.aliyuncs.com:3717/contractNotifyInfo?replicaSet=mgset-4977005";
            string mongodbDatabase = "contractNotifyInfo";
            string coll = "0x1ff70bb2147cf56c8b1ce0eb09323eb2b3f57916";
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            var txCount = collection.Find(new BsonDocument()).Count();

            client = null;

            return txCount;
        }
    }
}
