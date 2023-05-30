using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Knd.Printer.FujiNetworkPrinter.Helpers
{
    public class KeyHelper
    {
        public static RSAParameters LoadPkcsRsaPrivateKey(string path)
        {
            string s = File.ReadAllText(path);
            StringBuilder build = new StringBuilder(s);

            build.Replace("-----BEGIN RSA PRIVATE KEY-----", "");
            build.Replace("-----END RSA PRIVATE KEY-----", "");

            s = build.ToString().Trim();
            byte[] binKey = Convert.FromBase64String(s);

            AsnParser parser = new AsnParser(binKey);
            AsnKeyParser keyParser = new AsnKeyParser(parser);

            RSAParameters privateKey = keyParser.ParsePkcsRSAPrivateKey();

            return privateKey;
        }
        public static string SegmentEncryption(RSACryptoServiceProvider rsa, string s)
        {
            int size = 110;
            int length = s.Length - 1;
            int pages = (length % size) == 0 ? length / size : length / size + 1;
            IDictionary<string, string> dic = new Dictionary<string, string>();

            for (int i = 0; i < pages; i++)
            {
                var value = i < pages - 1 ? s.Substring(i * size, size) : s.Substring(i * size);
                string data = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(value), false));
                dic.Add(i.ToString(), data);
            }
            string json = JsonConvert.SerializeObject(dic);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
        public static string Sign(RSACryptoServiceProvider rsa, string str)
        {
            //根据需要加签时的哈希算法转化成对应的hash字符节
            byte[] bt = Encoding.UTF8.GetBytes(str);
            var sha1 = new SHA1CryptoServiceProvider();
            byte[] rgbHash = sha1.ComputeHash(bt);

            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(rsa);
            formatter.SetHashAlgorithm("SHA1"); //此处是你需要加签的hash算法，需要和上边你计算的hash值的算法一致，不然会报错。
            byte[] inArray = formatter.CreateSignature(rgbHash);
            return Convert.ToBase64String(inArray);
        }
        internal static RSAParameters LoadPkcsRsaPublicKey(string pubKeyPath)
        {
            string s = File.ReadAllText(pubKeyPath); // 从文件读取 
            StringBuilder build = new StringBuilder(s);

            // 去掉头尾

            build.Replace("-----BEGIN RSA PUBLIC KEY-----", "");
            build.Replace("-----END RSA PUBLIC KEY-----", "");

            s = build.ToString().Trim();
            byte[] binKey = Convert.FromBase64String(s);  // Base64解码

            AsnParser parser = new AsnParser(binKey); // 现在已经是AsnParser能认识的数据了 
            AsnKeyParser keyParser = new AsnKeyParser(parser);  // 就刚加的构造孙数

            RSAParameters publicKey = keyParser.ParsePkcsRSAPublicKey();  // 还是用刚加的解析函数

            return publicKey; // 公钥已经得到，可以尽情的用RSACryptoServiceProvider了。 
        }
    }
}
