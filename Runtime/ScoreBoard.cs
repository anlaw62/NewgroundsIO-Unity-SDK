using Newtonsoft.Json;
using UnityEngine.Scripting;
namespace Newgrounds
{
    public class Score
    {
        [Preserve, JsonConstructor]
        internal Score()
        {

        }
        public string FormattedValue => formatted_value;
        public string Tag => tag;
        public User User => user;
        public int Value => value;

        [JsonProperty]
        private string formatted_value;
        [JsonProperty]
        private string tag;
        [JsonProperty]
        private User user;
        [JsonProperty]
        private int value;
    }
    public enum Period
    {
        Day,
        Week,
        Month,
        Year
    }
}