using Newtonsoft.Json;
namespace Newgrounds
{

    public partial class Session
    {
        public bool Expired => expired;
        public string Id { get => id; set => id = value; }
        public string PassportUrl => passport_url;
        public User User => user;

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