using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Events;
using SpeedrunMod.Utils;

namespace SpeedrunMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("SliceCraft.MenuLib")]
// ReSharper disable once ClassNeverInstantiated.Global
internal class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;

    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        
        ModConfig.Initialize(Config);
        GlobalGame.canSkipDialogue = ModConfig.EnableDialogueSkip.Value;

        _harmony.PatchAll();

        SceneLoadedEvent.RegisterEvent();
        MenuInitializedEvent.RegisterEvent();

        VersionChecker.CheckForUpdatesAsync();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
