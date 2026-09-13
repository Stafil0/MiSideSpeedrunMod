using System;
using System.Linq;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Snapshots;
using UnityEngine;

namespace SpeedrunMod.Overlay.Modules;

internal sealed class RapidFireOverlayModule : IOverlayModule
{
    internal static readonly RapidFireOverlayModule Instance = new();

    public string Name => "Rapid Fire";

    public string GroupKey => "Core";

    public TimeSpan UpdateInterval { get; } = TimeSpan.FromSeconds(Math.Max(0f, OverlayConfig.OverlayLogInterval.Value));

    public void Reset()
    {
    }

    public IOverlaySnapshot Update()
    {
        var lines = RapidFire.GetActiveHps(Time.realtimeSinceStartup)
            .Select(row => $"{row.key}: {row.hps} HPS")
            .ToArray();

        if (lines.Length == 0)
        {
            return EmptyOverlaySnapshot.Instance;
        }

        return new TextOverlaySnapshot(string.Join("\n", lines));
    }
}
