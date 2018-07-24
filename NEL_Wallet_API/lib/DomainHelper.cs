﻿using System;
using System.Linq;

namespace NEL_Wallet_API.lib
{
    public class DomainHelper
    {
        public static readonly System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();

        public static Hash256 nameHash(string domain)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(domain);
            return new Hash256(sha256.ComputeHash(data));
        }
        public static Hash256 nameHashSub(byte[] roothash, string subdomain)
        {
            var bs = System.Text.Encoding.UTF8.GetBytes(subdomain);
            if (bs.Length == 0)
                return roothash;

            var domain = sha256.ComputeHash(bs).Concat(roothash).ToArray();
            return new Hash256(sha256.ComputeHash(domain));
        }
        public static Hash256 nameHashFull(string domain, string parent)
        {
            var ps = nameHash(parent);
            return nameHashSub(ps.data, domain);
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
