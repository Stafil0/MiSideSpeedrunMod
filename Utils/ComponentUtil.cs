using UnityEngine;

namespace SpeedrunMod.Utils;

public static class ComponentUtil
{
    public static string GetName(this Component component) => component == null 
        ? "<null>" 
        : $"{component.GetType().Name}/{component.gameObject.name}";
}
