using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;
namespace Newgrounds
{

    internal class SaveSlot
    {
        [Preserve, JsonConstructor]
        public SaveSlot()
        {

        }
        public int Id => id;
        public string Url => url;
        [JsonProperty]
        private int id;
        [JsonProperty]
        private string url;
    }
   internal class SaveData
    {
        public Dictionary<int, int> MedalProgress;
        public string Data;
    }
}