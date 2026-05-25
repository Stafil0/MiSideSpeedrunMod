using UnityEngine;

namespace SpeedrunMod.Utils;

public static class ComponentUtil
{
    public static string GetName(this Component component) => component == null 
        ? "<null>" 
        : $"{component.GetType().Name}/{component.gameObject.name}";

    internal static bool IsTrigger(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        foreach (Component component in gameObject.GetComponentsInParent<Component>(true))
        {
            if (component.GetType().Name.StartsWith("Trigger_"))
            {
                return true;
            }
        }

        return false;
    }
}
