using System;
using SpeedrunMod.RevealSystems.Colliders;
using SpeedrunMod.RevealSystems.Interactables;
using SpeedrunMod.RevealSystems.Triggers;
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
                Triggers.Hide();
            }

            if (Colliders.IsRevealing())
            {
                Colliders.Hide();
            }

            if (Interactables.IsRevealing())
            {
                Interactables.Hide();
            }
        }

        if (Triggers.IsRevealing())
        {
            Triggers.Clear();
            Plugin.Log.LogInfo("Revealing newly loaded triggers");
            Triggers.Reveal();
        }

        if (Colliders.IsRevealing())
        {
            Colliders.Clear();
            Plugin.Log.LogInfo("Revealing newly loaded physics colliders");
            Colliders.Reveal();
        }

        if (Interactables.IsRevealing())
        {
            Interactables.Clear();
            Plugin.Log.LogInfo("Revealing newly loaded interactable colliders");
            Interactables.Reveal();
        }
    }
}
