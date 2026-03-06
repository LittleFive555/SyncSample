using UnityEngine;

namespace SyncSample.Client
{
    public static class Logger
    {
        public static void Log(string message)
        {
            Debug.Log($"<color=blue>[Client]</color> {message}");
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning($"<color=blue>[Client]</color> {message}");
        }

        public static void LogError(string message)
        {
            Debug.LogError($"<color=blue>[Client]</color> {message}");
        }
    }
}