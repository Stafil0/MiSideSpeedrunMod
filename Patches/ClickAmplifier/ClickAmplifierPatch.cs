using System.Collections.Generic;
using HarmonyLib;
using SpeedrunMod.Configs;
using UnityEngine;

namespace SpeedrunMod.Patches.ClickAmplifier;

[HarmonyPatch]
internal static class ClickAmplifierPatch
{
    private const float HpsWindowSeconds = 1f;

    private static readonly Dictionary<KeyCode, KeyState> States = new();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetKeyDown), typeof(KeyCode))]
    private static void GetKeyDownPostfix(KeyCode key, ref bool __result)
    {
        if (!ClickAmplifierConfig.IsTracked(key))
        {
            return;
        }

        __result = Process(key, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnityEngine.Input), nameof(UnityEngine.Input.GetMouseButtonDown), typeof(int))]
    private static void GetMouseButtonDownPostfix(int button, ref bool __result)
    {
        if (button < 0 || button > 6)
        {
            return;
        }

        var key = KeyCode.Mouse0 + button;
        if (!ClickAmplifierConfig.IsTracked(key))
        {
            return;
        }

        __result = Process(key, __result);
    }

    internal static IEnumerable<(KeyCode key, int hps)> GetActiveHps(float now)
    {
        foreach (var key in ClickAmplifierConfig.GetTrackedKeys())
        {
            if (!States.TryGetValue(key, out var state))
            {
                continue;
            }

            var hits = state.CountHits(now);
            if (hits == 0 && state.Pending <= 0)
            {
                continue;
            }

            yield return (key, hits);
        }
    }

    private static bool Process(KeyCode key, bool originalDown)
    {
        var now = Time.realtimeSinceStartup;
        var frame = Time.frameCount;
        var state = GetState(key);

        if (state.LastFrame == frame)
        {
            return state.CachedDown;
        }

        state.LastFrame = frame;

        if (originalDown)
        {
            // ponytail: refill to N, don't stack; a >70Hz real stream would otherwise
            // queue a post-mash turbo tail. Raise this cap if testers want stacked bursts.
            state.Pending = ClickAmplifierConfig.GetSyntheticsPerPress();
        }

        var emit = false;
        if (now >= state.NextAllowed && (originalDown || state.Pending > 0))
        {
            emit = true;
            if (!originalDown)
            {
                state.Pending--;
            }

            state.NextAllowed = now + ClickAmplifierConfig.MinIntervalSeconds;
            state.RecordHit(now);
        }

        state.CachedDown = emit;
        return emit;
    }

    private static KeyState GetState(KeyCode key)
    {
        if (!States.TryGetValue(key, out var state))
        {
            state = new KeyState();
            States[key] = state;
        }

        return state;
    }

    private sealed class KeyState
    {
        internal int LastFrame = int.MinValue;
        internal bool CachedDown;
        internal float NextAllowed;
        internal int Pending;
        private readonly Queue<float> _hits = new();

        internal void RecordHit(float now)
        {
            _hits.Enqueue(now);
            Prune(now);
        }

        internal int CountHits(float now)
        {
            Prune(now);
            return _hits.Count;
        }

        private void Prune(float now)
        {
            var cutoff = now - HpsWindowSeconds;
            while (_hits.Count > 0 && _hits.Peek() < cutoff)
            {
                _hits.Dequeue();
            }
        }
    }
}
