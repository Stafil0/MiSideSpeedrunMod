using System.Collections.Concurrent;
using UnityEngine;

namespace SpeedrunMod.Utils;

public static class LogUtil
{
    private static ConcurrentDictionary<string, float> _lastLogTimes = new();

    public static void LogInfo(
        this BepInEx.Logging.ManualLogSource source, 
        string message, 
        string context,
        float throttleSeconds = 0f)
    {
        var now = Time.realtimeSinceStartup;
        if (throttleSeconds > 0f)
        {
            if (now - _lastLogTimes.GetOrAdd(context, now) < throttleSeconds) return;
        }

        source.LogInfo($"[{context}] {message}");
        _lastLogTimes.AddOrUpdate(context, now, (_, _) => now);
    }
}