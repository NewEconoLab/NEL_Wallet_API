using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Wallet_API.Controllers
{
    public class AuctionService
    {
        public mongoHelper mh { set; get; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public AuctionRecharge auctionRecharge { get; set; }


        public JArray hasTx(string txid)
        {
            bool issucces = queryHasTxFromBlock("tx", new JObject() { { "txid", txid } }.ToString());

            return new JArray() { { new JObject() { { "issucces", issucces } } } };
        }

        public JArray hasContract(string txid)
        {
            JArray queryRes = queryNotifyFromBlock("notify", new JObject() { { "txid", txid } }.ToString());
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { new JObject() { { "vmstate", ""},{ "displayNameList", new JArray() { } } } };
            }
            
            string[] res = queryRes.Where(p => ((JArray)p["notifications"]).Count() != 0).SelectMany(p =>
            {
                JArray pArr = (JArray)p["notifications"];
                return pArr.Select(pp => pp["state"]["value"][0]["value"].ToString()).Select(pp => pp.Hexstring2String()).ToArray();
            }).ToArray();


            string vmstate = queryRes[0]["vmstate"].ToString();
            return new JArray() { new JObject() { { "vmstate", vmstate }, { "displayNameList", new JArray() { res } } } };
        }

        public JArray rechargeAndTransfer(string txhex1, string txhex2)
        {
            return auctionRecharge.rechargeAndTransfer(txhex1, txhex2);
        }

        public JArray getRechargeAndTransfer(string txid)
        {
            return auctionRecharge.getRechargeAndTransfer(txid);
        }
        
        private bool queryHasTxFromBlock(string coll, string filter)
        {
            return mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, coll, filter) >= 1;
        }
        private JArray queryNotifyFromBlock(string coll, string filter)
        {
            return mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, coll, filter);
        }
        
        private Dictionary<string, long> getBlockTime(long[] blockindexArr)
        {
            JObject queryFilter = MongoFieldHelper.toFilter(blockindexArr, "index", "$or");
            JObject returnFilter = MongoFieldHelper.toReturn(new string[] { "index", "time" });
            JArray blocktimeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", returnFilter.ToString(), queryFilter.ToString());
            return blocktimeRes.ToDictionary(key => key["index"].ToString(), val => long.Parse(val["time"].ToString()));
        }

    }
}
