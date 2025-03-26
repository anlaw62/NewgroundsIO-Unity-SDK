using UnityEngine;
namespace Newgrounds
{
    public static class Debug
    {
        private const string Prefix = "[NGIO]";
        public static void LogError(string message)
        {
          UnityEngine.Debug.LogError($"{Prefix} {message}");
        }
    }
}