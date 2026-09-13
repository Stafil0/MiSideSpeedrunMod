using System.Collections.Generic;
using SpeedrunMod.Configs;
using UnityEngine;

namespace SpeedrunMod.Inputs;

internal static class RapidFireInputs
{
    internal const int MaxHps = 70;
    internal const float MinIntervalSeconds = 1f / MaxHps;

    private static readonly Dictionary<KeyCode, RapidFireInputsKeyState> States = new();

    internal static bool Process(KeyCode key, bool realDown)
    {
        if (!RapidFireInputsConfig.IsTracked(key))
        {
            return realDown;
        }

        var now = Time.realtimeSinceStartup;
        var frame = Time.frameCount;
        var state = GetState(key);

        // GetKeyDown stays true for every poll this Unity frame. Latch once.
        if (state.Frame == frame)
        {
            return state.DownThisFrame;
        }

        state.Frame = frame;

        if (realDown)
        {
            // ponytail: refill FollowUpsLeft, don't stack; a >70Hz real stream would otherwise
            // queue a post-mash turbo tail. Raise this cap if testers want stacked bursts.
            state.FollowUpsLeft = RapidFireInputsConfig.GetFollowUps();
        }

        var emit = false;
        if (now >= state.NextEmitAt && (realDown || state.FollowUpsLeft > 0))
        {
            emit = true;
            if (!realDown)
            {
                state.FollowUpsLeft--;
            }

            state.NextEmitAt = now + MinIntervalSeconds;
            state.RecordHit(now);
        }

        state.DownThisFrame = emit;
        return emit;
    }

    internal static IEnumerable<(KeyCode key, int hps)> GetActiveHps(float now)
    {
        foreach (var key in RapidFireInputsConfig.GetTrackedKeys())
        {
            if (!States.TryGetValue(key, out var state))
            {
                continue;
            }

            var hits = state.CountHits(now);
            if (hits == 0 && state.FollowUpsLeft <= 0)
            {
                continue;
            }

            yield return (key, hits);
        }
    }

    private static RapidFireInputsKeyState GetState(KeyCode key)
    {
        if (!States.TryGetValue(key, out var state))
        {
            state = new RapidFireInputsKeyState();
            States[key] = state;
        }

        return state;
    }
}
