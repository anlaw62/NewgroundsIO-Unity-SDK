using Newtonsoft.Json;
using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{
   
    public  class User
    {
        public int Id => id;
        public string Name => name;
        public bool Suppoerter => supporter;
        public string Url => url;

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