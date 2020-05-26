using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PM.Common.Encryption
{
    public class EncryptionFactory
    {
        #region (MD5)不可逆加密

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="strText">被加密字符串</param>
        /// <returns>加密后字符串</returns>
        public static string Md5Encrypt(string strText)
        {
            MD5 mD = MD5.Create();
            byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(strText));
            StringBuilder stringBuilder = new StringBuilder();
            byte[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                byte b = array2[i];
                string text = b.ToString("x");
                if (text.Length == 1)
                {
                    stringBuilder.Append("0");
                }
                stringBuilder.Append(text);
            }
            return stringBuilder.ToString();
        }

        #endregion

        #region 非对称加密

        /// <summary>
        /// RSA字符串加密
        /// </summary>
        /// <param name="source">源字符串 明文</param>
        /// <param name="key">密匙</param>
        /// <returns>加密遇到错误将会返回原字符串</returns>
        public static string RsaEncrypt(string source, string keyFilePath)
        {
            byte[] en = Encoding.UTF8.GetBytes(source);

            RSAParameters param = PemConverter.GetPemPublicKey(keyFilePath);
            RSACryptoServiceProvider rsasp = new RSACryptoServiceProvider();
            rsasp.ImportParameters(param);
            byte[] encryptedData = rsasp.Encrypt(en, false);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// RSA字符串解密
        /// </summary>
        /// <param name="encryptString">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>遇到解密失败将会返回原字符串</returns>
        public static string RsaDecrypt(string encryptString, string keyFilePath)
        {
            byte[] de = Convert.FromBase64String(encryptString);

            RSAParameters param = PemConverter.GetPemPrivateKey(keyFilePath);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(param);

            byte[] decryptedData = rsa.Decrypt(de, false);

            return Encoding.UTF8.GetString(decryptedData);
        }

        #endregion

        #region 对称加密

        //默认密钥向量
        private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="encryptKey">加密密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public static string EncryptDES(string encryptString, string encryptKey)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return encryptString;
            }
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        public static string DecryptDES(string decryptString, string decryptKey)
        {
            try
            {
                decryptKey = decryptKey.Substring(0, 8);
                decryptKey = decryptKey.PadRight(8, ' ');
                byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey);
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();

                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return "";
            }
        }

        #endregion

        /// <summary>
        /// 编码密码，目前Sha1
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="key">solt</param>
        /// <param name="appVer">应用版本，可能会Sha2加密</param>
        /// <returns></returns>
        public static string SHAEncode(string password, string key, int appVer = 0)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(key))
                return string.Empty;
            var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            var encodePw = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(encodePw);
        }
    }
}
