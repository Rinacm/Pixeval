using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mako.Model;

namespace Mako.Console
{
    public static class Program
    {
        public static async Task Main()
        {

        }

        public static string EncryptedId(string id)
        {
            var b1 = Encoding.UTF8.GetBytes("3go8&$8*3*3h0k(2)2");
            var b2 = Encoding.UTF8.GetBytes(id);

            for (int i = 0; i < b2.Length; i++)
            {
                b2[i] = (byte) (b2[i] ^ b1[i % b1.Length]);
            }

            using var m = new MD5CryptoServiceProvider();
            var resultBytes = m.ComputeHash(b2);
            return Convert.ToBase64String(resultBytes)[..^1].Replace("/", "_").Replace("+", "-");
        }
    }
}