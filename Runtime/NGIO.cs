using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

namespace Newgrounds
{
    public class NGIO
    {
        public string AppId { get; private set; }
        public string AesKey { get; private set; }
        internal const string GATEWAY_URI = "https://www.newgrounds.io/gateway_v3.php";
        public NGIO(string appId, string aesKey)
        {
            AppId = appId;
            AesKey = aesKey;
        }
      
    }
}