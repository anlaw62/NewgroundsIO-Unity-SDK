namespace Newgrounds
{
    internal static class Debug
    {
        private const string Prefix = "[NGIO]";
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError($"{Prefix} {message}");
        }
        public static void Log(string message)
        {
            UnityEngine.Debug.Log($"{Prefix} {message}");
        }
    }
}