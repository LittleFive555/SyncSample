using UnityEngine;

namespace SyncSample.Server
{
    public static class Logger
    {
        public static void Log(string message)
        {
            Debug.Log($"<color=green>[Server]</color> {message}");
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning($"<color=green>[Server]</color> {message}");
        }

        public static void LogError(string message)
        {
            Debug.LogError($"<color=green>[Server]</color> {message}");
        }
    }
}