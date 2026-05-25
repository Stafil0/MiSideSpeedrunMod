using HarmonyLib;
using SpeedrunMod.RevealSystems;
using UnityEngine;

namespace SpeedrunMod.Patches;

/// <summary>
/// When updateCast is enabled, Trigger_Event.Start copies BoxCollider then destroys the collider.
/// Cache that half-extent here so trigger reveal can draw the real OverlapBox volume later.
/// </summary>
[HarmonyPatch(typeof(Trigger_Event), nameof(Trigger_Event.Start))]
internal static class Trigger_EventRevealPatch
{
    [HarmonyPrefix]
    private static void StartPrefix(Trigger_Event __instance)
    {
        BoxCollider box = __instance.GetComponent<BoxCollider>();
        if (box == null)
        {
            return;
        }

        Triggers.CacheEventCollider(__instance, box);
    }
}
