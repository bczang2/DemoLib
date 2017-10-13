using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.MD5
{
    public class MD5Helper
    {
        public static string MD5(string inputVal)
        {
            if (string.IsNullOrWhiteSpace(inputVal))
            {
                return string.Empty;
            }

            return MD5(Encoding.UTF8.GetBytes(inputVal));
        }

        /// <summary>
        /// md5 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string MD5(byte[] inputVal)
        {
            if (inputVal == null || inputVal.Length == 0)
            {
                return string.Empty;
            }

            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] source = md5.ComputeHash(inputVal);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < source.Length; i++)
            {
                sBuilder.Append(source[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string MD5(int inputVal)
        {
            return MD5(inputVal.ToString());
        }

        public static string MD5(long inputVal)
        {
            return MD5(inputVal.ToString());
        }

        public static string MD5(double inputVal)
        {
            return MD5(inputVal.ToString());
        }
    }
}
