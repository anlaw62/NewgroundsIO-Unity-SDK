using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        internal class ExecuteObject
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
            public void Encrypt(byte[] key)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] input = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
                        cryptoStream.Write(input, 0, input.Length);
                        cryptoStream.FlushFinalBlock();
                        secure = Convert.ToBase64String(ms.ToArray());
                    }
                    echo = default;
                    component = default;
                    parameters = default;

                }
            }
        }

    }
}