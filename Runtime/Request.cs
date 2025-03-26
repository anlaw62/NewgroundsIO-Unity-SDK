using Newtonsoft.Json;
using UnityEngine;
using Unity.Serialization;
using Unity.Serialization.Json;

using Unity.Properties;
using System.Collections.Generic;
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

        [CreateProperty]
        private string debug;
        [CreateProperty]
        private string session_id;
        [CreateProperty]
        private string app_id;


        [GeneratePropertyBag]
        internal partial class ExecuteComponent
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
        }

    }
}