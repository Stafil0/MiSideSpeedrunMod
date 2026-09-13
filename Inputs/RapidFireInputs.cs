using System.Collections.Generic;
using SpeedrunMod.Configs;
using UnityEngine;

namespace SpeedrunMod.Inputs;

internal static class RapidFireInputs
{
    internal const int MaxHps = 70;
    internal const float MinIntervalSeconds = 1f / MaxHps;

    private const float HpsWindowSeconds = 1f;

    private static readonly Dictionary<KeyCode, KeyState> States = new();

    internal static bool Process(KeyCode key, bool originalDown)
    {
        if (!RapidFireConfig.IsTracked(key))
        {
            return originalDown;
        }

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
            state.Pending = RapidFireConfig.GetSyntheticsPerPress();
        }

        var emit = false;
        if (now >= state.NextAllowed && (originalDown || state.Pending > 0))
        {
            emit = true;
            if (!originalDown)
            {
                state.Pending--;
            }

            state.NextAllowed = now + MinIntervalSeconds;
            state.RecordHit(now);
        }

        state.CachedDown = emit;
        return emit;
    }

    internal static bool ProcessMouseButton(int button, bool originalDown)
    {
        if (button < 0 || button > 6)
        {
            return originalDown;
        }

        return Process(KeyCode.Mouse0 + button, originalDown);
    }

    internal static IEnumerable<(KeyCode key, int hps)> GetActiveHps(float now)
    {
        foreach (var key in RapidFireConfig.GetTrackedKeys())
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
