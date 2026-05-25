using System.Reflection;
using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class InteractiveUtil
{
    private static readonly FieldInfo InteractiveFieldInfo = typeof(ObjectInteractive).GetField(
        "objectInteractive",
        BindingFlags.Instance | BindingFlags.NonPublic);

    internal static float ResolveInteractionRadius(GameObject player)
    {
        if (player == null)
        {
            return 0;
        }

        var capsule = player.GetComponent<CapsuleCollider>() ?? player.GetComponentInChildren<CapsuleCollider>(true);
        if (capsule != null)
        {
            return capsule.radius * 2f;
        }

        return 0;
    }

    internal static float ResolveInteractionRange(PlayerMove playerMove) => playerMove != null ? playerMove.distanceCast : 0f;

    internal static float ResolveInteractionRange(Camera camera, Collider collider)
    {
        if (camera == null || collider == null)
        {
            return float.PositiveInfinity;
        }

        Vector3 closest = collider.bounds.ClosestPoint(camera.transform.position);
        return Vector3.Distance(camera.transform.position, closest);
    }

    internal static bool IsInInteractionRange(Camera camera, Collider collider, float distanceCast)
    {
        if (distanceCast <= 0f)
        {
            return false;
        }

        return ResolveInteractionRange(camera, collider) <= distanceCast;
    }

    internal static bool IsInteractable(Collider collider)
    {
        if (collider == null)
        {
            return false;
        }

        ObjectInteractive objectInteractive = collider.GetComponentInParent<ObjectInteractive>(true);
        if (objectInteractive != null && IsInteractiveCollider(collider, objectInteractive))
        {
            return true;
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

    private static bool IsInteractiveCollider(Collider collider, ObjectInteractive objectInteractive)
    {
        if (collider.gameObject == objectInteractive.gameObject)
        {
            return true;
        }

        if (collider.transform.IsChildOf(objectInteractive.transform))
        {
            return true;
        }

        if (InteractiveFieldInfo?.GetValue(objectInteractive) is GameObject target && target != null)
        {
            if (collider.gameObject == target || collider.transform.IsChildOf(target.transform))
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
        _ => typeName.EndsWith("Interactive")
            && !typeName.StartsWith("Trigger_")
            && !typeName.Contains("CaseInfo"),
    };
}
