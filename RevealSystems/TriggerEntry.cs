using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems;

internal sealed class TriggerEntry
{
    private Component _source;

    internal GameObject Shape { get; set; }
    internal bool HasShape { get; set; }
    internal Component Source
    {
        get => _source;
        set
        {
            _source = value;
            Name = value.GetName();
        }
    }

    internal Text Label { get; set; }
    internal string Type { get; set; }
    internal string Name { get; private set; }
    internal Vector3? ColliderSize { get; set; }
    internal Vector3? ColliderCenter { get; set; }

    internal TriggerEntry(Component source, string type, GameObject shape = null, Text label = null, Vector3? colliderSize = null, Vector3? colliderCenter = null)
    {
        Source = source;
        Type = type;
        Shape = shape;
        Label = label;
        ColliderSize = colliderSize;
        ColliderCenter = colliderCenter;
    }
}
