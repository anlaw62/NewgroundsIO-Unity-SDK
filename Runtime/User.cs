using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{
    [GeneratePropertyBag]
    public partial class User
    {
        public int Id => id;
        public string Name => name;
        public bool Suppoerter => supporter;
        public string Url => url;

        [CreateProperty]
        private int id;
        [CreateProperty]    
        private string name;
        [CreateProperty]
        private bool supporter;
        [CreateProperty]
        private string url;
    }
}