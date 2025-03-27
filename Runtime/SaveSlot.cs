using Newtonsoft.Json;
namespace Newgrounds
{

    public class SaveSlot
    {
        public int Id => id;
        public string Url => url;
        [JsonProperty]
        private int id;
        [JsonProperty]
        private string url;
    }
}