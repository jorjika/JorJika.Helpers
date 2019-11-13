using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JorJika.Helpers
{
    public static class SecurityHelper
    {
        public static string GenerateMD5Hash(string str)
        {
            var md5Obj = new MD5CryptoServiceProvider();
            var bytesToHash = Encoding.ASCII.GetBytes(str);
            var resultBytes = md5Obj.ComputeHash(bytesToHash);
            return resultBytes.Aggregate("", (current, b) => current + b.ToString("x2"));
        }

        public static string GenerateSHA256Hash(string text)
        {
            UTF8Encoding ue = new UTF8Encoding();
            byte[] message = ue.GetBytes(text);
            string hex = "";

            using (SHA256Managed hashString = new SHA256Managed())
            {
                var hashValue = hashString.ComputeHash(message);
                hex = hashValue.Aggregate(hex, (current, x) => current + $"{x:x2}");
            }

            return hex;
        }

    }
}
