using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunMod.Utils;

internal sealed class ActionQueue
{
    private readonly Queue<Func<bool>> _queue = new();
    private readonly Func<float> _timeProvider;

    public ActionQueue(Func<float> timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public ActionQueue() : this(() => Time.realtimeSinceStartup)
    {
    }

    public void Clear() => _queue.Clear();

    public void Enqueue(Action work)
    {
        _queue.Enqueue(() =>
        {
            work();
            return true;
        });
    }

    public void EnqueueWait(float seconds)
    {
        float? deadline = null;
        _queue.Enqueue(() =>
        {
            deadline ??= _timeProvider() + seconds;
            return _timeProvider() >= deadline.Value;
        });
    }

    /// <summary>Completes when <paramref name="ready"/> returns true; then runs <paramref name="work"/> once.</summary>
    public void EnqueueConditional(Func<bool> ready, Action work)
    {
        _queue.Enqueue(() =>
        {
            if (!ready())
                return false;

            work();
            return true;
        });
    }

    public void Tick()
    {
        while (_queue.Count > 0)
        {
            if (!_queue.Peek().Invoke())
                break;

            _queue.Dequeue();
        }
    }
}
