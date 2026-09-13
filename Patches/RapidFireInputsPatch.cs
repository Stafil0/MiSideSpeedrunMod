using HarmonyLib;
using SpeedrunMod.Inputs;
using UnityEngine;

namespace SpeedrunMod.Patches;

[HarmonyPatch]
internal static class RapidFireInputsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetKeyDown), typeof(KeyCode))]
    private static void GetKeyDownPostfix(KeyCode key, ref bool __result)
    {
        __result = RapidFireInputs.Process(key, __result);
    }
}
