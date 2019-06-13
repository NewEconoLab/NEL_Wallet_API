using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEL_Wallet_API.lib
{
    public class MongoFieldHelper
    {
        public static JObject newRegexFilter(string val, string field="")
        {
            if(field != "")
            {
                return new JObject { { field, new JObject() { { "$regex", val }, { "$options", "i" } } } };
            }
            return new JObject() { { "$regex", val }, { "$options", "i" } };
        }
        public static JObject newExistEqFilter(string val, string field = "")
        {
            if(field != "")
            {
                return new JObject { { field, new JObject { { "$exists", true }, { "$eq", val } } } };
            }
            return new JObject { { "$exists", true }, { "$eq", val } };
        }
        public static JObject newNoExistEqFilter(string val, string field, bool addOr=true)
        {
            return new JObject { { "$or", newNoExistEqFilter(val, field) } };
        }
        public static JArray newNoExistEqFilter(string val, string field)
        {
            return new JArray
            {
                new JObject{{field, new JObject { { "$exists", false } } } },
                new JObject{{field, new JObject { { "$exists", true }, { "$ne", val } } } }
            };
        }
    
        public static JObject likeFilter(string key, string regex, string notIncludeValue="")
        {
            if(notIncludeValue != "")
            {
                return new JObject() { { key, new JObject() { { "$ne", notIncludeValue }, { "$regex", regex }, { "$options", "i" } } } };
            }
            return new JObject() { { key, new JObject() { { "$regex", regex }, { "$options", "i" } } } };
        }
        public static JObject toFilter(long[] blockindexArr, string field, string logicalOperator = "$or")
        {
            if (blockindexArr.Count() == 1)
            {
                return new JObject() { { field, blockindexArr[0] } };
            }
            return new JObject() { { logicalOperator, new JArray() { blockindexArr.Select(item => new JObject() { { field, item } }).ToArray() } } };
        }
        public static JObject toFilter(string[] blockindexArr, string field, string logicalOperator = "$or")
        {
            if (blockindexArr.Count() == 1)
            {
                return new JObject() { { field, blockindexArr[0] } };
            }
            return new JObject() { { logicalOperator, new JArray() { blockindexArr.Select(item => new JObject() { { field, item } }).ToArray() } } };
        }
        public static JObject toReturn(string[] fieldArr)
        {
            JObject obj = new JObject();
            foreach (var field in fieldArr)
            {
                obj.Add(field, 1);
            }
            return obj;
        }
        public static JObject toSort(string[] fieldArr, bool order = false)
        {
            int flag = order ? 1 : -1;
            JObject obj = new JObject();
            foreach (var field in fieldArr)
            {
                obj.Add(field, flag);
            }
            return obj;
        }
        
    }
}
