using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CityHall.Synchronous
{
    public class Password
    {
        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "";
            }

            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            return string.Join("", md5.ComputeHash(inputBytes).Select(b => b.ToString("X2").ToLower()));
        }
    }
}
