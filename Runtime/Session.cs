using Newtonsoft.Json;
namespace Newgrounds
{

    public sealed class Session
    {
        public bool Expired 
        {
            get => expired;
            internal set => expired = value;
        }
        public string Id 
        {
            get => id;
            internal set => id = value;
        }
        public string PassportUrl 
        {
            get => passport_url;
            internal set => passport_url = value;
        }
        public User User 
        {
            get => user;
            internal set => user = value;
        }

        [JsonProperty]
        private bool expired;
        [JsonProperty]
        private string id;
        [JsonProperty]
        private string passport_url;
        [JsonProperty]
        private User user;

    }
}