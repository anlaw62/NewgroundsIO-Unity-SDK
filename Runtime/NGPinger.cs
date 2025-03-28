using Cysharp.Threading.Tasks;
using Newgrounds;
using UnityEngine;
namespace Newgrounds
{
    public class NGPinger : MonoBehaviour
    {
        private void Awake()
        {
            NGIO.Instance.Ping().Forget();
        }
        private void Update()
        {
         if(Application.isFocused)   NGIO.Instance.Ping().Forget();
        }
    }
}