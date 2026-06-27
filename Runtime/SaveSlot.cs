using Newtonsoft.Json;
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
}