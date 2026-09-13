using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunMod.Inputs;

internal sealed class RapidFireInputsKeyState
{
    private const float HpsWindowSeconds = 1f;

    internal int Frame = int.MinValue;
    internal bool DownThisFrame;
    internal float NextEmitAt;
    internal int FollowUpsLeft;
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
