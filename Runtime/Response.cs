using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Scripting;
namespace Newgrounds
{

    internal  class Response<ResultDataType>
    {
        [Preserve,JsonConstructor]
      public Response()
        {

        }
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
            [Preserve, JsonConstructor]
            public ErrorObject()
            {

            }
            public int Code => code;
            public string Message => message;
            [JsonProperty]
            private int code;
            [JsonProperty]
            private string message;
        }
  
        public  class ResultObject
        {
            [Preserve, JsonConstructor]
            public ResultObject()
            {

            }
            public ResultDataType this[string key]{
                get
                {
                    return JsonConvert.DeserializeObject<ResultDataType>(JsonConvert.SerializeObject(data[key]));
                }
            } 
            [JsonProperty]
            private Dictionary<string, object> data;
        }
    }
}