using Unity.Properties;
using UnityEngine;
namespace Newgrounds
{
    [GeneratePropertyBag]
    public partial class SaveSlot
    {
        public int Id => id;
        public string Data => data;
        [CreateProperty]
        private string data;
        [CreateProperty]
        private int id;
    }
}