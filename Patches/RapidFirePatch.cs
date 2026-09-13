using HarmonyLib;
using UnityEngine;

namespace SpeedrunMod.Patches;

[HarmonyPatch]
internal static class RapidFirePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetKeyDown), typeof(KeyCode))]
    private static void GetKeyDownPostfix(KeyCode key, ref bool __result)
    {
        __result = RapidFire.Process(key, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetMouseButtonDown), typeof(int))]
    private static void GetMouseButtonDownPostfix(int button, ref bool __result)
    {
        __result = RapidFire.ProcessMouseButton(button, __result);
    }
}
