using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{
    [GeneratePropertyBag]
    internal partial class Response<ResultDataType>
    {
        public bool Success => success;
        public ErrorObject Error => error;
        public ResultObject Result => result;
        [CreateProperty]
        private bool success;
        [CreateProperty]
        private ErrorObject error;
        [CreateProperty]
        private ResultObject result;
       
        [GeneratePropertyBag]
        public partial class ErrorObject
        {
            public int Code => code;
            public string Message => message;
            [CreateProperty]
            private int code;
            [CreateProperty]
            private string message;
        }
        [GeneratePropertyBag]
        public partial class ResultObject
        {
            public Dictionary<string, ResultDataType> Data => data;
            [CreateProperty]
            private Dictionary<string, ResultDataType> data;
        }
    }
}