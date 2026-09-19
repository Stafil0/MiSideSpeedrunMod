using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class RapidFireInputsConfig
{
    private const string DefaultTrackedKeys = "Space,E,Q";

    internal static ConfigEntry<int> Synthetics;
    internal static ConfigEntry<string> TrackedKeys;

    private static string _parsedRaw;
    private static string[] _tracked = [];

    internal static void Initialize(ConfigFile configFile)
    {
        Synthetics = configFile.Bind(
            "RapidFireInputs",
            "Synthetics",
            3,
            "Extra GetKeyDown edges queued after each real press of a tracked key. Developer tuning only; not shown in the in-game menu.");

        TrackedKeys = configFile.Bind(
            "RapidFireInputs",
            "TrackedKeys",
            DefaultTrackedKeys,
            "Comma-separated ids to amplify inside allowed Updates (KeyCode names). Interactive follows Space/E; MouseClick follows Mouse0. Developer tuning only; not shown in the in-game menu.");
    }

    internal static int GetSynthetics()
    {
        return Math.Max(0, Synthetics.Value);
    }

    internal static bool IsTracked(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        EnsureParsed();
        
        if (ContainsTracked(id))
        {
            return true;
        }

        // InputManager axes used by interact / click-to-advance, bound to tracked keys.
        if (id.Equals("Interactive", StringComparison.OrdinalIgnoreCase))
        {
            return ContainsTracked(nameof(KeyCode.Space)) || ContainsTracked(nameof(KeyCode.E));
        }

        if (id.Equals("MouseClick", StringComparison.OrdinalIgnoreCase))
        {
            return ContainsTracked(nameof(KeyCode.Mouse0));
        }

        return false;
    }

    private static void EnsureParsed()
    {
        var raw = TrackedKeys?.Value ?? string.Empty;
        if (raw == _parsedRaw)
        {
            return;
        }

        _parsedRaw = raw;
        var ids = new List<string>();
        foreach (var part in raw.Split(','))
        {
            var name = part.Trim();
            if (name.Length == 0)
            {
                continue;
            }

            if (Enum.TryParse(name, true, out KeyCode key) && key != KeyCode.None)
            {
                name = key.ToString();
            }

            if (ContainsIgnoreCase(ids, name))
            {
                continue;
            }

            ids.Add(name);
        }

        _tracked = ids.ToArray();
    }

    private static bool ContainsTracked(string id)
    {
        foreach (var tracked in _tracked)
        {
            if (tracked.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsIgnoreCase(List<string> ids, string name)
    {
        foreach (var id in ids)
        {
            if (id.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
