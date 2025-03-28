using Newtonsoft.Json;
using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{

    public sealed class User
    {

        public int Id
        {
            get => id;
            internal set => id = value;
        }
        public string Name
        {
            get => name;
            internal set => name = value;
        }
        public bool Supporter
        {
            get => supporter;
            internal set=> supporter = value;
        }
        public string Url
        {
            get => url;
            internal set => url = value;
        }

        [JsonProperty]
        private int id;
        [JsonProperty]
        private string name;
        [JsonProperty]
        private bool supporter;
        [JsonProperty]
        private string url;
    }
}