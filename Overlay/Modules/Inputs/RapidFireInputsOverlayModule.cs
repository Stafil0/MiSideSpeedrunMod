using System;
using System.Text;
using SpeedrunMod.Configs;
using SpeedrunMod.Inputs;
using SpeedrunMod.Overlay.Snapshots;
using UnityEngine;

namespace SpeedrunMod.Overlay.Modules;

internal sealed class RapidFireInputsOverlayModule : IOverlayModule
{
    internal static readonly RapidFireInputsOverlayModule Instance = new();

    public string Name => "Rapid Fire";

    public string GroupKey => "Core";

    public TimeSpan UpdateInterval { get; } = TimeSpan.FromSeconds(Math.Max(0f, OverlayConfig.OverlayLogInterval.Value));

    public void Reset()
    {
    }

    public IOverlaySnapshot Update()
    {
        var text = new StringBuilder();
        foreach (var (id, realHps, syntheticHps) in RapidFireInputs.GetHps(Time.realtimeSinceStartup))
        {
            text.Append(id)
                .Append(": ")
                .Append(realHps)
                .Append(" rHPS / ")
                .Append(syntheticHps)
                .Append(" sHPS")
                .AppendLine();
        }

        if (text.Length == 0)
        {
            return EmptyOverlaySnapshot.Instance;
        }

        return new TextOverlaySnapshot(text.ToString());
    }
}
