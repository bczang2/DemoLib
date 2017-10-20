using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.SHA
{
    public class SHAUtil
    {
        public static string SHA1(string inputVal)
        {
            if (string.IsNullOrWhiteSpace(inputVal))
            {
                return string.Empty;
            }

            HashAlgorithm sha1 = new SHA1CryptoServiceProvider();
            byte[] bts = sha1.ComputeHash(Encoding.UTF8.GetBytes(inputVal));
            StringBuilder temp = new StringBuilder();
            if (bts != null && bts.Length > 0)
            {
                for (int i = 0; i < bts.Length; i++)
                {
                    temp.AppendFormat("{0:x2}", bts[i]);
                }
            }

            return temp.ToString();
        }

        public static string SHA256(string inputVal)
        {
            if (string.IsNullOrWhiteSpace(inputVal))
            {
                return string.Empty;
            }

            SHA256 sha256 = new SHA256Managed();
            byte[] bts = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputVal));
            StringBuilder temp = new StringBuilder();
            if (bts != null && bts.Length > 0)
            {
                for (int i = 0; i < bts.Length; i++)
                {
                    temp.AppendFormat("{0:x2}", bts[i]);
                }
            }

            return temp.ToString();
        }

        public static string SHA384(string inputVal)
        {
            if (string.IsNullOrWhiteSpace(inputVal))
            {
                return string.Empty;
            }

            SHA384 sha384 = new SHA384Managed();
            byte[] bts = sha384.ComputeHash(Encoding.UTF8.GetBytes(inputVal));
            StringBuilder temp = new StringBuilder();
            if (bts != null && bts.Length > 0)
            {
                for (int i = 0; i < bts.Length; i++)
                {
                    temp.AppendFormat("{0:x2}", bts[i]);
                }
            }

            return temp.ToString();
        }

        public static string SHA512(string inputVal)
        {
            if (string.IsNullOrWhiteSpace(inputVal))
            {
                return string.Empty;
            }

            SHA512 sha512 = new SHA512Managed();
            byte[] bts = sha512.ComputeHash(Encoding.UTF8.GetBytes(inputVal));
            StringBuilder temp = new StringBuilder();
            if (bts != null && bts.Length > 0)
            {
                for (int i = 0; i < bts.Length; i++)
                {
                    temp.AppendFormat("{0:x2}", bts[i]);
                }
            }

            return temp.ToString();
        }
    }
}
