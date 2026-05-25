using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class InteractiveUtil
{
    internal static bool IsInteractable(Collider collider)
    {
        if (collider == null)
        {
            return false;
        }

        foreach (Component component in collider.gameObject.GetComponentsInParent<Component>(true))
        {
            if (IsInteractiveType(component.GetType().Name))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInteractiveType(string typeName) => typeName switch
    {
        nameof(ObjectInteractive) => true,
        nameof(ItemInteractive) => true,
        nameof(ObjectInteractiveItemTake) => true,
        nameof(ObjectInteractiveGroup) => true,
        nameof(MakeManeken_Interactive) => true,
        nameof(Metroidvania_Interactive) => true,
        nameof(Location14_QuestInteractive) => true,
        _ => typeName.EndsWith("Interactive") && !typeName.StartsWith("Trigger_"),
    };
}
