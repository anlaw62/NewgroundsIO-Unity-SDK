using Newtonsoft.Json;
using UnityEngine;
using Unity.Serialization;
using Unity.Serialization.Json;

using Unity.Properties;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;
namespace Newgrounds
{
    [GeneratePropertyBag]
    internal partial class Request
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
            set=>execute = value;
        }
        [CreateProperty]
        private string debug;
        [CreateProperty]
        private string session_id;
        [CreateProperty]
        private string app_id;
        [CreateProperty]
        private ExecuteObject execute;
        [GeneratePropertyBag]
        internal partial class ExecuteObject
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
            [CreateProperty]
            private Dictionary<string, object> parameters;
            [CreateProperty]
            private string echo;
            [CreateProperty]
            protected string component;
            [CreateProperty]
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
                        byte[] input = Encoding.UTF8.GetBytes(JsonSerialization.ToJson(this));
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