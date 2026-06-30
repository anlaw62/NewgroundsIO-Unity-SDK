using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
namespace Newgrounds
{
    public class NGPinger : MonoBehaviour
    {
        private void Awake()
        {
            NGIO.Ping().Forget();
            StartCoroutine(Ping());
        }
        private IEnumerator Ping()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(60f * 5f);
              
                    NGIO.Ping().Forget();

            }
        }
     
    }
}