using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class FastResetConfig
{
    internal static ConfigEntry<bool> EnableFastReset;
    internal static ConfigEntry<float> HoldSeconds;
    internal static ConfigEntry<KeyCode> ResetChapterKey;
    internal static ConfigEntry<KeyCode> PreviousChapterKey;
    internal static ConfigEntry<KeyCode> NextChapterKey;
    internal static ConfigEntry<KeyCode> NewGameKey;

    internal static void Initialize(ConfigFile configFile)
    {
        EnableFastReset = configFile.Bind(
            "FastReset",
            "EnableFastReset",
            true,
            "When true, hold ResetKey in-game to reset current practice chapter; hold with Shift for full run reset (main menu).");

        HoldSeconds = configFile.Bind(
            "FastReset",
            "HoldSeconds",
            1f,
            "Seconds the chapter key must be held before reset fires.");

        ResetChapterKey = configFile.Bind(
            "FastReset",
            "ResetChapterKey",
            KeyCode.R,
            "Key to hold for chapter reset (see EnableFastReset).");
        
        PreviousChapterKey = configFile.Bind(
            "FastReset",
            "PreviousChapterKey",
            KeyCode.LeftControl,
            "Key to hold for previous chapter reset (see EnableFastReset).");

        NextChapterKey = configFile.Bind(
            "FastReset",
            "NextChapterKey",
            KeyCode.LeftAlt,
            "Key to hold for next chapter reset (see EnableFastReset).");
        
        NewGameKey = configFile.Bind(
            "FastReset",
            "NewGameKey",
            KeyCode.LeftShift,
            "Key to hold for new game reset (see EnableFastReset).");
    }
}
