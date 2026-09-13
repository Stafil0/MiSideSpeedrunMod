using System;
using System.Linq;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Snapshots;
using SpeedrunMod.Patches.ClickAmplifier;
using UnityEngine;

namespace SpeedrunMod.Overlay.Modules.ClickAmplifier;

internal sealed class ClickAmplifierOverlayModule : IOverlayModule
{
    internal static readonly ClickAmplifierOverlayModule Instance = new();

    public string Name => "Click Amplifier";

    public string GroupKey => "Core";

    public TimeSpan UpdateInterval { get; } = TimeSpan.FromSeconds(Math.Max(0f, OverlayConfig.OverlayLogInterval.Value));

    public void Reset()
    {
    }

    public IOverlaySnapshot Update()
    {
        var lines = ClickAmplifierPatch.GetActiveHps(Time.realtimeSinceStartup)
            .Select(row => $"{row.key}: {row.hps} HPS")
            .ToArray();

        if (lines.Length == 0)
        {
            return EmptyOverlaySnapshot.Instance;
        }

        return new TextOverlaySnapshot(string.Join("\n", lines));
    }
}
