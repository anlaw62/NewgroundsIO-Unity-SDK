using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;
namespace Newgrounds
{
    public class Medal
    {
        [Preserve, JsonConstructor]
        internal Medal()
        {

        }
        public string Description => description;
        /// <summary>
        /// Difficulty from 0 to 5(including)
        /// </summary>
        public int Difficulty => difficulty - 1;
        public string IconUrl => icon;
        public int Id => id;
        public string Name => name;
        public bool Secret => secret;
        public bool Unlocked => unlocked;
        public int Value => value;

        [JsonProperty]
        private string description;
        [JsonProperty]
        private int difficulty;
        [JsonProperty]
        private string icon;
        [JsonProperty]
        private int id;
        [JsonProperty]
        private string name;
        [JsonProperty]
        private bool secret;
        [JsonProperty]
        private bool unlocked;
        [JsonProperty]
        private int value;
    }
}