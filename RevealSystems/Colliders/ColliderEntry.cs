using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems.Colliders;

internal sealed class ColliderEntry
{
    internal Collider Source { get; }
    internal string Type { get; }
    internal bool InSync { get; }
    internal bool HasShape { get; set; }
    internal GameObject Shape { get; set; }
    internal Text Label { get; set; }
    internal string Name { get; }

    internal ColliderEntry(Collider source, string type, bool syncTransform)
    {
        Source = source;
        Type = type;
        InSync = syncTransform;
        Name = source.GetName();
    }
}
