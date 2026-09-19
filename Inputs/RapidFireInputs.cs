using System;
using System.Collections.Generic;
using SpeedrunMod.Configs;
using UnityEngine;

namespace SpeedrunMod.Inputs;

internal static class RapidFireInputs
{
    internal const int MaxHps = 70;
    internal const float MinIntervalSeconds = 1f / MaxHps;

    private static readonly Dictionary<string, KeyState> States = new();

    internal static bool Process(KeyCode key, bool keyDown)
    {
        return Process(key.ToString(), keyDown);
    }

    internal static bool Process(string id, bool keyDown)
    {
        if (!RapidFireInputsConfig.IsTracked(id))
        {
            return keyDown;
        }

        var now = Time.realtimeSinceStartup;
        var frame = Time.frameCount;
        var state = GetState(id);

        // GetKeyDown stays true for every poll this Unity frame. Latch once.
        if (state.Frame == frame)
        {
            return state.Down;
        }

        state.Frame = frame;

        if (keyDown)
        {
            // Reset LastPressAt, don't stack; a >70Hz real stream would otherwise
            // queue a post-mash turbo tail. Raise this cap if testers want stacked bursts.
            state.LastPressAt = now;
        }

        var emit = false;
        if (state.CanEmit(now) && (keyDown || state.CountSyntheticsLeft(now) > 0))
        {
            emit = true;
            state.RecordHit(now, synthetic: !keyDown);
            if (state.CountSyntheticsLeft(now) == 0)
            {
                state.LastPressAt = float.NaN;
            }
        }

        state.Down = emit;
        return emit;
    }

    internal static IEnumerable<(string id, int realHps, int syntheticHps)> GetHps(float now)
    {
        foreach (var pair in States)
        {
            yield return (pair.Key, pair.Value.CountRealHps(now), pair.Value.CountSyntheticHps(now));
        }
    }

    private static KeyState GetState(string id)
    {
        if (!States.TryGetValue(id, out var state))
        {
            state = new KeyState();
            States[id] = state;
        }

        return state;
    }

    private sealed class KeyState
    {
        private const float HpsWindowSeconds = 1f;

        internal int Frame = int.MinValue;
        internal bool Down;
        internal float LastPressAt = float.NaN;
        private readonly List<(float at, bool synthetic)> _hits = new();

        internal bool CanEmit(float now)
        {
            return _hits.Count == 0 || now - _hits[^1].at >= MinIntervalSeconds;
        }

        internal void RecordHit(float now, bool synthetic)
        {
            _hits.Add((now, synthetic));
            Prune(now);
        }

        internal int CountRealHps(float now)
        {
            return CountHps(now, synthetic: false);
        }

        internal int CountSyntheticHps(float now)
        {
            return CountHps(now, synthetic: true);
        }

        internal int CountSyntheticsLeft(float now)
        {
            if (float.IsNaN(LastPressAt))
            {
                return 0;
            }

            Prune(now);
            var emitted = 0;
            foreach (var hit in _hits)
            {
                if (hit.at > LastPressAt)
                {
                    emitted++;
                }
            }

            return Math.Max(0, RapidFireInputsConfig.GetSyntheticsPerPress() - emitted);
        }

        private int CountHps(float now, bool synthetic)
        {
            Prune(now);
            var n = 0;
            foreach (var hit in _hits)
            {
                if (hit.synthetic == synthetic)
                {
                    n++;
                }
            }

            return n;
        }

        private void Prune(float now)
        {
            var cutoff = now - HpsWindowSeconds;
            var drop = 0;
            while (drop < _hits.Count && _hits[drop].at < cutoff)
            {
                drop++;
            }

            if (drop > 0)
            {
                _hits.RemoveRange(0, drop);
            }
        }
    }
}
