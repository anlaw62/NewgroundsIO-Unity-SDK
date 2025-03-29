using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace Newgrounds
{

    internal class Request
    {
        public string AppId
        {
            get => app_id;
            set => app_id = value;
        }
        public string SessionId
        {
            get => session_id;
            set => session_id = value;
        }
        public string Debug
        {
            get => debug;
            set => debug = value;
        }
        public ExecuteObject ExecuteObj
        {
            get => execute;
            set => execute = value;
        }
        [JsonProperty]
        private string debug;
        [JsonProperty]
        private string session_id;
        [JsonProperty]
        private string app_id;
        [JsonProperty]
        private ExecuteObject execute;

        public class ExecuteObject
        {

            public string Echo
            {
                get => echo;
                set => echo = value;
            }
            public string Component
            {
                get => component;
                set => component = value;
            }
            public Dictionary<string, object> Parameters
            {
                get => parameters;
                set => parameters = value;
            }
            [JsonProperty]
            private Dictionary<string, object> parameters;
            [JsonProperty]
            private string echo;
            [JsonProperty]
            protected string component;
            [JsonProperty]
            private string secure;
          private byte[] EncryptAES128(string plainText, byte[] key, byte[] iv)
            {


                using Aes aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using MemoryStream msEncrypt = new();
                using CryptoStream csEncrypt = new(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write);
                using StreamWriter swEncrypt = new(csEncrypt);

                swEncrypt.Write(plainText);
                swEncrypt.Flush();
                csEncrypt.FlushFinalBlock();

                return msEncrypt.ToArray();
            }

            public void Encrypt(byte[] aesKey,JsonSerializerSettings settings)
            {
                using Aes aesAlg = Aes.Create();
                aesAlg.Key = aesKey;
                aesAlg.GenerateIV();

                byte[] aesEncrypted = EncryptAES128(JsonConvert.SerializeObject(this,settings), aesAlg.Key, aesAlg.IV);
                byte[] encryptedBytes = new byte[aesAlg.IV.Length + aesEncrypted.Length];

                Buffer.BlockCopy(aesAlg.IV, 0, encryptedBytes, 0, aesAlg.IV.Length);
                Buffer.BlockCopy(aesEncrypted, 0, encryptedBytes, aesAlg.IV.Length, aesEncrypted.Length);

              secure = Convert.ToBase64String(encryptedBytes);
            }
        }

    }
}