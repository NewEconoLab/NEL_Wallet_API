using NEL_Wallet_API.Controllers;
using NEL_Wallet_API.lib;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEL_Wallet_API.Service
{
    public class UtxoService
    {
        public string mongodbConnStr { set; get; }
        public string mongodbDatabase { set; get; }
        public string cgasUtxoCol { set; get; }
        public mongoHelper mh { set; get; }

        public JArray getAvailableUtxos(string address, decimal amount)
        {
            //
            string findstr = new JObject() { { "markAddress", address} }.ToString();
            JArray queryRes = mh.GetData(mongodbConnStr, mongodbDatabase, cgasUtxoCol, findstr);
            if(queryRes != null && queryRes.Count != 0)
            {
                return new JArray() {
                    queryRes.Select(p => {
                        return new JObject(){
                            { "txid",p["txid"]},
                            { "n",p["n"]},
                            { "value",p["value"]},
                        };
                    }).ToArray()
                };
            }

            // 
            findstr = new JObject() { { "markAddress", "0" },{ "lockAddress", "0"} }.ToString();
            queryRes = mh.GetData(mongodbConnStr, mongodbDatabase, cgasUtxoCol, findstr);
            if(queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }

            decimal sum = queryRes.Sum(p => decimal.Parse(p["value"].ToString()));
            if (amount > sum)
            {
                return new JArray() { };
            }
            return selectUtxo(queryRes, address, amount);
        }

        private JArray selectUtxo(JArray queyRes, string address, decimal amount)
        {
            JArray res = new JArray() { };
            long nowtime = TimeHelper.GetTimeStamp();
            decimal calcSum = 0;
            JToken[] jtArr = queyRes.OrderByDescending(p => decimal.Parse(p["value"].ToString())).ToArray();
            foreach (JToken jt in jtArr)
            {
                if (calcSum >= amount) break;

                // 
                JObject jo = (JObject)jt;
                jo.Remove("markAddress");
                jo.Add("markAddress", address);
                jo.Remove("markTime");
                jo.Add("markTime", nowtime);
                string newdata = jo.ToString();
                string findstr = new JObject() { { "txid", jt["txid"] }, { "n", jt["n"] }, { "value", jt["value"] }, { "markAddress", "0" } }.ToString();
                if (!markUtxo(findstr, newdata)) continue;
                calcSum += decimal.Parse(jt["value"].ToString());
                res.Add(new JObject(){
                            { "txid",jt["txid"]},
                            { "n",jt["n"]},
                            { "value",jt["value"]},
                        });
            }
            if (res.Sum(p => decimal.Parse(p["value"].ToString())) < amount) return new JArray() { };
            return res;
        }

        private bool markUtxo(string findstr, string newdata)
        {
            string res = mh.ReplaceData(mongodbConnStr, mongodbDatabase, cgasUtxoCol, findstr, newdata);
            // 检查返回值,是否标记成功

            return res == "suc";
        }
    }
}
