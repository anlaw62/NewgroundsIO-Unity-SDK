using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{
    [GeneratePropertyBag]
    public partial class Session
    {
        public bool Expired => expired;
        public string Id => id;
        public string PassportUrl => passport_url;
        public User User => user;

        [CreateProperty]
        private bool expired;
        [CreateProperty]
        private string id;
        [CreateProperty]
        private string passport_url;
        [CreateProperty]
        private User user;

    }
}