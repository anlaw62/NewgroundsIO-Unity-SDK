using System;
using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{
    [GeneratePropertyBag]
    internal partial class Response
    {
        public bool Success => success;
        public ErrorObject Error => error;
        [CreateProperty]
        private bool success;
        [CreateProperty]
        private ErrorObject error;

       
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
    }
}