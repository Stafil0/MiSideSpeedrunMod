using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems.Interactables;

internal sealed class InteractableEntry
{
    internal Collider Source { get; }
    internal bool HasShape { get; set; }
    internal GameObject Shape { get; set; }
    internal Text Label { get; set; }
    internal string Text { get; set; }
    internal InteractableState? State { get; set; }
    internal string Name { get; }

    internal InteractableEntry(Collider source)
    {
        Source = source;
        Name = source.GetName();
    }
}
