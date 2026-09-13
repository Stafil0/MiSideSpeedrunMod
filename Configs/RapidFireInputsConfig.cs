using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class RapidFireInputsConfig
{
    private const string DefaultTrackedKeys = "Space,E";

    internal static ConfigEntry<int> FollowUps;
    internal static ConfigEntry<string> TrackedKeys;

    private static string _parsedRaw;
    private static KeyCode[] _tracked = [];

    internal static void Initialize(ConfigFile configFile)
    {
        FollowUps = configFile.Bind(
            "RapidFireInputs",
            "FollowUps",
            3,
            "Extra GetKeyDown edges queued after each real press of a tracked key. Developer tuning only; not shown in the in-game menu.");

        TrackedKeys = configFile.Bind(
            "RapidFireInputs",
            "TrackedKeys",
            DefaultTrackedKeys,
            "Comma-separated Unity KeyCode names to amplify and cap (e.g. Space,E). Keyboard only. Developer tuning only; not shown in the in-game menu.");
    }

    internal static int GetFollowUps()
    {
        return Math.Max(0, FollowUps.Value);
    }

    internal static bool IsTracked(KeyCode key)
    {
        EnsureParsed();
        foreach (var tracked in _tracked)
        {
            if (tracked == key)
            {
                return true;
            }
        }

        return false;
    }

    internal static KeyCode[] GetTrackedKeys()
    {
        EnsureParsed();
        return _tracked;
    }

    private static void EnsureParsed()
    {
        var raw = TrackedKeys?.Value ?? string.Empty;
        if (raw == _parsedRaw)
        {
            return;
        }

        _parsedRaw = raw;
        var keys = new List<KeyCode>();
        foreach (var part in raw.Split(','))
        {
            var name = part.Trim();
            if (name.Length == 0)
            {
                continue;
            }

            if (!Enum.TryParse(name, true, out KeyCode key) || key == KeyCode.None || IsMouse(key) || keys.Contains(key))
            {
                continue;
            }

            keys.Add(key);
        }

        _tracked = keys.ToArray();
    }

    private static bool IsMouse(KeyCode key)
    {
        return key is >= KeyCode.Mouse0 and <= KeyCode.Mouse6;
    }
}
