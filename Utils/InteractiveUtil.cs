using System.Reflection;
using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class InteractiveUtil
{
    private static readonly FieldInfo InteractiveFieldInfo = typeof(ObjectInteractive).GetField(
        "objectInteractive",
        BindingFlags.Instance | BindingFlags.NonPublic);

    internal static bool IsInRange(Collider collider, PlayerMove playerMove)
    {
        GameObject cast = playerMove?.objectCastInteractive;
        if (cast == null || collider == null)
        {
            return false;
        }

        if (IsOnGameObject(collider, cast))
        {
            return true;
        }

        ObjectInteractive castInteractive = cast.GetComponent<ObjectInteractive>();
        if (castInteractive != null && IsInteractive(collider, castInteractive))
        {
            return true;
        }

        ObjectInteractive colliderInteractive = collider.GetComponentInParent<ObjectInteractive>(true);
        if (colliderInteractive == null)
        {
            return false;
        }

        if (cast == colliderInteractive.gameObject)
        {
            return true;
        }

        return IsInteractive(collider, colliderInteractive) && cast.transform.IsChildOf(colliderInteractive.transform);
    }

    internal static bool IsWithinReach(Collider collider, PlayerMove playerMove)
    {
        if (collider == null || playerMove == null)
        {
            return false;
        }

        ObjectInteractive interactive = collider.GetComponentInParent<ObjectInteractive>(true);
        if (interactive == null || interactive.distanceFloor <= 0f)
        {
            return false;
        }

        if (!TryResolveFloorDistance(playerMove.transform, collider, interactive, out float floorDistance))
        {
            return false;
        }

        return floorDistance <= interactive.distanceFloor;
    }

    internal static bool IsAimedAt(Camera camera, Collider collider, PlayerMove playerMove)
    {
        if (collider == null || playerMove == null)
        {
            return false;
        }

        if (!TryInteractRaycast(camera, playerMove, collider, out RaycastHit hit))
        {
            return false;
        }

        return hit.collider != null && AreInteractive(collider, hit.collider);
    }

    internal static bool IsInteractive(Collider collider)
    {
        if (collider == null)
        {
            return false;
        }

        ObjectInteractive objectInteractive = collider.GetComponentInParent<ObjectInteractive>(true);
        if (objectInteractive != null && IsInteractive(collider, objectInteractive))
        {
            return true;
        }

        foreach (Component component in collider.gameObject.GetComponentsInParent<Component>(true))
        {
            if (component == null)
            {
                continue;
            }

            if (IsInteractiveType(component.GetType().Name))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool TryResolveDistance(PlayerMove playerMove, Collider collider, out float distance)
    {
        distance = 0f;

        if (playerMove == null)
        {
            return false;
        }

        return TryResolveDistance(playerMove.transform, collider, out distance);
    }

    internal static bool TryResolveDistance(Transform origin, Collider collider, out float distance)
    {
        distance = 0f;

        if (origin == null || collider == null)
        {
            return false;
        }

        Vector3 closest = collider.bounds.ClosestPoint(origin.position);
        distance = Vector3.Distance(origin.position, closest);
        return true;
    }

    private static bool TryResolveFloorDistance(
        Transform origin,
        Collider collider,
        ObjectInteractive interactive,
        out float distance)
    {
        distance = 0f;

        if (origin == null || collider == null || interactive == null)
        {
            return false;
        }

        Vector3 playerPosition = origin.position;
        Vector3 closest = collider is MeshCollider { convex: false }
            ? collider.bounds.ClosestPoint(playerPosition)
            : collider.ClosestPoint(playerPosition);
        distance = GlobalAM.DistanceFloor(playerPosition, closest);
        return true;
    }

    private static bool TryResolveMaxDistance(
        Transform castFrom,
        Collider collider,
        float castRadius,
        out float maxDistance)
    {
        maxDistance = 0f;

        if (castFrom == null || collider == null)
        {
            return false;
        }

        Bounds bounds = collider.bounds;
        Ray ray = new Ray(castFrom.position, castFrom.forward);

        if (bounds.IntersectRay(ray, out float enter))
        {
            maxDistance = enter + bounds.extents.magnitude + castRadius;
            return true;
        }

        if (bounds.Contains(castFrom.position))
        {
            maxDistance = bounds.extents.magnitude + castRadius;
            return true;
        }

        return false;
    }

    private static bool TryInteractRaycast(
        Camera fallbackCamera,
        PlayerMove playerMove,
        Collider target,
        out RaycastHit hit)
    {
        hit = default;

        if (playerMove == null || target == null)
        {
            return false;
        }

        Transform castFrom = ResolveCastTransform(playerMove, fallbackCamera);
        if (castFrom == null)
        {
            return false;
        }

        CapsuleCollider capsule = playerMove.GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            return false;
        }

        if (!TryResolveMaxDistance(castFrom, target, capsule.radius, out float maxDistance))
        {
            return false;
        }

        return Physics.Raycast(
            castFrom.position,
            castFrom.forward,
            out hit,
            maxDistance,
            playerMove.castHead);
    }

    private static Transform ResolveCastTransform(PlayerMove playerMove, Camera fallbackCamera)
    {
        if (playerMove?.mainCamera != null && playerMove.mainCamera.gameObject.activeInHierarchy)
        {
            return playerMove.mainCamera;
        }

        return fallbackCamera != null ? fallbackCamera.transform : null;
    }

    private static bool AreInteractive(Collider a, Collider b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a == b)
        {
            return true;
        }

        ObjectInteractive interactiveA = a.GetComponentInParent<ObjectInteractive>(true);
        ObjectInteractive interactiveB = b.GetComponentInParent<ObjectInteractive>(true);
        if (interactiveA != null && interactiveB != null)
        {
            return interactiveA == interactiveB;
        }

        return a.transform.IsChildOf(b.transform) || b.transform.IsChildOf(a.transform);
    }

    private static bool IsOnGameObject(Collider collider, GameObject gameObject)
    {
        return collider.gameObject == gameObject || collider.transform.IsChildOf(gameObject.transform);
    }

    private static bool IsInteractive(Collider collider, ObjectInteractive objectInteractive)
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
