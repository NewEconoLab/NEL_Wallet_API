using System;
using System.Linq;
using System.Security.Cryptography;

namespace NEL_Wallet_API.lib
{
    public class DomainHelper
    {
        private const string ROOT_NEO = ".neo";
        private const string ROOT_TEST = ".neo";
        public static bool IsSupportRoot(string domain)
        {
            return domain.EndsWith(ROOT_NEO) || domain.EndsWith(ROOT_TEST);
        }
        public static string getFullDomain4Neo(string domain)
        {
            //if (domain.EndsWith(ROOT_NEO)) return domain;
            return domain + ROOT_NEO;
        }
        public static string getFullDomain4Test(string domain)
        {
            //if (domain.EndsWith(ROOT_TEST)) return domain;
            return domain + ROOT_TEST;
        }
        public static string[] getFullDomain(string domain)
        {
            return new string[] { getFullDomain4Neo(domain), getFullDomain4Test(domain) };
        }
        public static string getDefalutFullDomain(string domain)
        {
            if (domain.EndsWith(ROOT_NEO)) return domain;
            if (domain.EndsWith(ROOT_TEST)) return domain;
            return domain + ROOT_NEO;
        }
        public static Hash256 nameHash(string domain)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(domain);
            return new Hash256(SHA256.Create().ComputeHash(data));
        }
        public static Hash256 nameHashSub(byte[] roothash, string subdomain)
        {
            var bs = System.Text.Encoding.UTF8.GetBytes(subdomain);
            if (bs.Length == 0)
                return roothash;
            SHA256 sha256 = SHA256.Create();
            var domain = sha256.ComputeHash(bs).Concat(roothash).ToArray();
            return new Hash256(sha256.ComputeHash(domain));
        }
        public static Hash256 nameHashFull(string domain, string parent)
        {
            var ps = nameHash(parent);
            return nameHashSub(ps.data, domain);
        }
        public static string nameHashFullDomain(string fulldomain)
        {
            int split = fulldomain.LastIndexOf(".");
            if(split == -1)
            {
                return nameHash(fulldomain).ToString();
            }
            var domain = fulldomain.Substring(0, split);
            var parent = fulldomain.Substring(split+1);
            return nameHashFull(domain, parent).ToString();
        }
    }
    public class Hash256 : IComparable<Hash256>
    {
        public Hash256(byte[] data)
        {
            if (data.Length != 32)
                throw new Exception("error length.");
            this.data = data;
        }
        public Hash256(string hexstr)
        {
            var bts = ThinNeo.Helper.HexString2Bytes(hexstr);
            if (bts.Length != 32)
                throw new Exception("error length.");
            this.data = bts.Reverse().ToArray();
        }
        public override string ToString()
        {
            return "0x" + ThinNeo.Helper.Bytes2HexString(this.data.Reverse().ToArray());
        }
        public byte[] data;

        public int CompareTo(Hash256 other)
        {
            byte[] x = data;
            byte[] y = other.data;
            for (int i = x.Length - 1; i >= 0; i--)
            {
                if (x[i] > y[i])
                    return 1;
                if (x[i] < y[i])
                    return -1;
            }
            return 0;
        }
        public override bool Equals(object obj)
        {
            return CompareTo(obj as Hash256) == 0;
        }

        public static implicit operator byte[] (Hash256 value)
        {
            return value.data;
        }
        public static implicit operator Hash256(byte[] value)
        {
            return new Hash256(value);
        }
    }
}
