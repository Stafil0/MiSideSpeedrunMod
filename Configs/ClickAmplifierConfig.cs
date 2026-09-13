using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class ClickAmplifierConfig
{
    internal const int MaxHps = 70;
    internal const float MinIntervalSeconds = 1f / MaxHps;

    private const string DefaultTrackedKeys = "Space,E,Mouse0";

    internal static ConfigEntry<int> SyntheticsPerPress;
    internal static ConfigEntry<string> TrackedKeys;

    private static string _parsedRaw;
    private static readonly List<KeyCode> TrackedList = new();
    private static readonly HashSet<KeyCode> TrackedSet = new();

    internal static void Initialize(ConfigFile configFile)
    {
        SyntheticsPerPress = configFile.Bind(
            "ClickAmplifier",
            "SyntheticsPerPress",
            3,
            "Synthetic follow-up edge-presses scheduled after each accepted real press of a tracked key. Developer tuning only; not shown in the in-game menu.");

        TrackedKeys = configFile.Bind(
            "ClickAmplifier",
            "TrackedKeys",
            DefaultTrackedKeys,
            "Comma-separated Unity KeyCode names to amplify and cap (e.g. Space,E,Mouse0). Developer tuning only; not shown in the in-game menu.");
    }

    internal static int GetSyntheticsPerPress()
    {
        return Math.Max(0, SyntheticsPerPress.Value);
    }

    internal static bool IsTracked(KeyCode key)
    {
        EnsureParsed();
        return TrackedSet.Contains(key);
    }

    internal static IReadOnlyList<KeyCode> GetTrackedKeys()
    {
        EnsureParsed();
        return TrackedList;
    }

    private static void EnsureParsed()
    {
        var raw = TrackedKeys?.Value ?? string.Empty;
        if (raw == _parsedRaw)
        {
            return;
        }

        _parsedRaw = raw;
        TrackedList.Clear();
        TrackedSet.Clear();

        foreach (var part in raw.Split(','))
        {
            var name = part.Trim();
            if (name.Length == 0)
            {
                continue;
            }

            if (!Enum.TryParse(name, true, out KeyCode key) || key == KeyCode.None || !TrackedSet.Add(key))
            {
                continue;
            }

            TrackedList.Add(key);
        }
    }
}
