using System;
using SpeedrunMod.RevealSystems;
using SpeedrunMod.Utils;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Events;

internal static class SceneLoadedEvent
{
    internal static event Action<Scene, LoadSceneMode> SceneLoaded;

    internal static void RegisterEvent()
    {
        SceneManager.sceneLoaded += (UnityEngine.Events.UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Plugin.Log.LogDebug($"Loading scene: {scene.name}");

        SceneLoaded?.Invoke(scene, mode);

        if (scene.name == "SceneMenu")
        {
            VersionText.Start();

            if (Triggers.IsRevealing())
            {
                Triggers.HideTriggers();
            }
        }
        
        if (Triggers.IsRevealing())
        {
            Triggers.ClearEntries(); 
            Plugin.Log.LogInfo("Revealing newly loaded triggers");
            Triggers.RevealTriggers();
        }
    }
}