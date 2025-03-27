using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{

    internal  class Response<ResultDataType>
    {
        public bool Success => success;
        public ErrorObject Error => error;
        public ResultObject Result => result;
        [JsonProperty]
        private bool success;
        [JsonProperty]
        private ErrorObject error;
        [JsonProperty]
        private ResultObject result;
       
       
        public  class ErrorObject
        {
            public int Code => code;
            public string Message => message;
            [JsonProperty]
            private int code;
            [JsonProperty]
            private string message;
        }
  
        public  class ResultObject
        {
            public Dictionary<string, ResultDataType> Data => data;
            [JsonProperty]
            private Dictionary<string, ResultDataType> data;
        }
    }
}