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
            if (component == null)
            {
                continue;
            }

            if (component.GetType().Name.StartsWith("Trigger_"))
            {
                return true;
            }
        }

        return false;
    }

    internal static GameObject FindIncludingInactive(string name)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && t.gameObject.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }

    internal static T FindIncludingInactive<T>(string name) where T : Component
    {
        GameObject go = FindIncludingInactive(name);
        return go != null ? go.GetComponent<T>() : null;
    }

    internal static void Enable(string name, bool enabled)
    {
        GameObject go = FindIncludingInactive(name);
        if (go != null && go.activeSelf != enabled)
        {
            go.SetActive(enabled);
        }
    }
}
