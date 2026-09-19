using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class RapidFireInputsConfig
{
    private const string DefaultTrackedKeys = "Space,E,Q";

    internal static ConfigEntry<int> SyntheticsPerPress;
    internal static ConfigEntry<string> TrackedKeys;

    private static string _parsedRaw;
    private static HashSet<string> _tracked = new(StringComparer.OrdinalIgnoreCase);

    internal static void Initialize(ConfigFile configFile)
    {
        SyntheticsPerPress = configFile.Bind(
            "RapidFireInputs",
            "SyntheticsPerPress",
            3,
            "Extra GetKeyDown edges queued after each real press of a tracked key. Developer tuning only; not shown in the in-game menu.");

        TrackedKeys = configFile.Bind(
            "RapidFireInputs",
            "TrackedKeys",
            DefaultTrackedKeys,
            "Comma-separated ids to amplify inside allowed Updates (KeyCode names). Interactive follows Space/E; MouseClick follows Mouse0. Developer tuning only; not shown in the in-game menu.");
    }

    internal static int GetSyntheticsPerPress()
    {
        return Math.Max(0, SyntheticsPerPress.Value);
    }

    internal static bool IsTracked(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        EnsureParsed();

        if (_tracked.Contains(id))
        {
            return true;
        }

        // InputManager axes used by interact / click-to-advance, bound to tracked keys.
        if (id.Equals("Interactive", StringComparison.OrdinalIgnoreCase))
        {
            return _tracked.Contains(nameof(KeyCode.Space)) || _tracked.Contains(nameof(KeyCode.E));
        }

        if (id.Equals("MouseClick", StringComparison.OrdinalIgnoreCase))
        {
            return _tracked.Contains(nameof(KeyCode.Mouse0));
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
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

            ids.Add(name);
        }

        _tracked = ids;
    }
}
