using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using SpeedrunMod.Inputs;
using UnityEngine;

namespace SpeedrunMod.Patches;

[HarmonyPatch]
internal static class RapidFireInputsPatch
{
    private static int _allowedUpdates;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetKeyDown), typeof(KeyCode))]
    private static void GetKeyDownPostfix(KeyCode key, ref bool __result)
    {
        if (Volatile.Read(ref _allowedUpdates) <= 0)
        {
            return;
        }

        __result = RapidFireInputs.Process(key, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetButtonDown), typeof(string))]
    private static void GetButtonDownPostfix(string buttonName, ref bool __result)
    {
        if (Volatile.Read(ref _allowedUpdates) <= 0)
        {
            return;
        }

        __result = RapidFireInputs.Process(buttonName, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetMouseButtonDown), typeof(int))]
    private static void GetMouseButtonDownPostfix(int button, ref bool __result)
    {
        if (Volatile.Read(ref _allowedUpdates) <= 0 || button != 0)
        {
            return;
        }

        __result = RapidFireInputs.Process("Mouse0", __result);
    }

    [HarmonyPatch]
    internal static class AllowedUpdates
    {
        // Interact / dialogue-skip / click-to-advance. Not menus, not PlayerMove
        // (BetterMovement jump), not Input_Event (arbitrary axis). Add types here.
        private static readonly Type[] Types =
        [
            typeof(ObjectInteractive),
            typeof(ItemInteractive),
            typeof(Metroidvania_Interactive),
            typeof(Tamagotchi_Dialogue),
            typeof(Dialogue_3DText),
            typeof(Location18_Novella),
            typeof(Location18_TicTacToe),
            typeof(Location19_TriggerClick),
        ];

        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var type in Types)
            {
                var update = AccessTools.Method(type, "Update");
                if (update != null)
                {
                    yield return update;
                }
            }
        }

        [HarmonyPrefix]
        private static void Prefix(ref bool __state)
        {
            Interlocked.Increment(ref _allowedUpdates);
            __state = true;
        }

        [HarmonyFinalizer]
        private static void Finalizer(bool __state)
        {
            if (!__state)
            {
                return;
            }

            Interlocked.Decrement(ref _allowedUpdates);
        }
    }
}
