using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.DES
{
    public class DESUtil
    {
        //默认密钥向量
        private static byte[] Keys = { 0x13, 0x24, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="encryptKey">加密密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串(base64)，失败返回源串</returns>
        public static string DESEncrypt_Base64(string encryptString, string encryptKey)
        {
            string encryptRes = encryptString;
            try
            {
                using (DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider())
                {
                    byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                    byte[] rgbIV = Keys;
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        using (CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                        {
                            cStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cStream.FlushFinalBlock();
                            encryptRes = Convert.ToBase64String(mStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("加密失败");
            }

            return encryptRes;
        }

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="encryptKey">加密密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串(base64)，失败返回源串</returns>
        public static string DESEncrypt_Hex(string encryptString, string encryptKey)
        {
            string encryptRes = encryptString;
            try
            {
                using (DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider())
                {
                    byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                    byte[] rgbIV = Keys;
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        using (CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                        {
                            cStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cStream.FlushFinalBlock();

                            StringBuilder sb = new StringBuilder();
                            foreach (byte b in mStream.ToArray())
                            {
                                sb.AppendFormat("{0:x2}", b);
                            }
                            encryptRes = sb.ToString();
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("加密失败");
            }

            return encryptRes;
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        public static string DESDecrypt_Base64(string decryptString, string decryptKey)
        {
            string decryptRes = decryptString;
            try
            {
                using (DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider())
                {
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                        byte[] rgbIV = Keys;
                        byte[] inputByteArray = Convert.FromBase64String(decryptString);
                        using (CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                        {
                            cStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cStream.FlushFinalBlock();
                            decryptRes = Encoding.UTF8.GetString(mStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("解密失败");
            }

            return decryptRes;
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        public static string DESDecrypt_Hex(string decryptString, string decryptKey)
        {
            string decryptRes = decryptString;
            try
            {
                using (DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider())
                {
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        byte[] inputByteArray = new byte[decryptString.Length / 2];
                        for (int x = 0; x < decryptString.Length / 2; x++)
                        {
                            int i = (Convert.ToInt32(decryptString.Substring(x * 2, 2), 16));
                            inputByteArray[x] = (byte)i;
                        }
                        byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                        byte[] rgbIV = Keys;
                        using (CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                        {
                            cStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cStream.FlushFinalBlock();
                            decryptRes = Encoding.UTF8.GetString(mStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("解密失败");
            }

            return decryptRes;
        }

        /// <summary>
        /// 创建加密签名
        /// </summary>
        /// <param name="userinfo"></param>
        /// <returns></returns>
        public static string CreateSignature_Base64(string userinfo)
        {
            var token = Guid.NewGuid().ToString("N").Substring(0, 8);
            return DESEncrypt_Base64(userinfo, token);
        }

        /// <summary>
        /// 创建加密签名
        /// </summary>
        /// <param name="userinfo"></param>
        /// <returns></returns>
        public static string CreateSignature_Hex(string userinfo)
        {
            var token = Guid.NewGuid().ToString("N").Substring(0, 8);
            return DESEncrypt_Hex(userinfo, token);
        }
    }
}
