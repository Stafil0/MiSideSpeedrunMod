using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class ColliderUtil
{
    internal static GameObject CreateShape(Collider collider, string name)
    {
        if (collider == null)
        {
            return null;
        }

        GameObject reveal = CreatePrimitive(collider);
        if (reveal == null)
        {
            return null;
        }

        reveal.name = name;
        ApplyTransform(reveal.transform, collider);
        Disable(reveal);

        return reveal;
    }

    internal static Vector3 ResolveLabelPosition(Collider collider)
    {
        if (collider == null)
        {
            return Vector3.zero;
        }

        return ResolveLabelPosition(collider.transform, collider.bounds);
    }

    internal static Vector3 ResolveLabelPosition(Transform labelParent, Bounds worldBounds)
    {
        if (labelParent == null)
        {
            return Vector3.zero;
        }

        return labelParent.InverseTransformPoint(worldBounds.center);
    }

    internal static void Disable(GameObject shape)
    {
        Collider revealCollider = shape.GetComponent<Collider>();
        if (revealCollider != null)
        {
            revealCollider.enabled = false;
        }
    }

    internal static GameObject CreatePrimitive(Collider collider)
    {
        return collider switch
        {
            BoxCollider => GameObject.CreatePrimitive(PrimitiveType.Cube),
            SphereCollider => GameObject.CreatePrimitive(PrimitiveType.Sphere),
            CapsuleCollider => GameObject.CreatePrimitive(PrimitiveType.Capsule),
            MeshCollider mc => CreateMeshReveal(mc),
            _ => GameObject.CreatePrimitive(PrimitiveType.Cube)
        };
    }

    private static GameObject CreateMeshReveal(MeshCollider meshCollider)
    {
        if (meshCollider.sharedMesh == null)
        {
            return null;
        }

        var go = new GameObject();
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = meshCollider.sharedMesh;
        go.AddComponent<MeshRenderer>();
        return go;
    }

    internal static void ApplyTransform(Transform reveal, Collider collider)
    {
        reveal.SetParent(collider.transform, false);
        reveal.localRotation = Quaternion.identity;

        switch (collider)
        {
            case BoxCollider box:
                reveal.localPosition = box.center;
                reveal.localScale = box.size;
                return;

            case SphereCollider sphere:
                reveal.localPosition = sphere.center;
                reveal.localScale = Vector3.one * (sphere.radius * 2f);
                return;

            case CapsuleCollider capsule:
                ApplyCapsuleTransform(reveal, capsule);
                break;

            case MeshCollider:
                reveal.SetParent(collider.transform, false);
                reveal.localPosition = Vector3.zero;
                reveal.localRotation = Quaternion.identity;
                reveal.localScale = Vector3.one;
                break;

            default:
                Bounds bounds = collider.bounds;
                reveal.SetParent(collider.transform, false);
                reveal.localRotation = Quaternion.identity;
                reveal.localPosition = collider.transform.InverseTransformPoint(bounds.center);
                reveal.localScale = BoundsToLocalScale(collider.transform, bounds.size);
                FitToColliderBounds(reveal, collider);
                break;
        }
    }

    private static void FitToColliderBounds(Transform reveal, Collider collider)
    {
        MeshRenderer renderer = reveal.GetComponent<MeshRenderer>();
        if (renderer == null || collider == null)
        {
            return;
        }

        Bounds target = collider.bounds;
        Bounds current = renderer.bounds;
        Vector3 scale = reveal.localScale;

        if (current.size.x > 1e-6f)
        {
            scale.x *= target.size.x / current.size.x;
        }

        if (current.size.y > 1e-6f)
        {
            scale.y *= target.size.y / current.size.y;
        }

        if (current.size.z > 1e-6f)
        {
            scale.z *= target.size.z / current.size.z;
        }

        reveal.localScale = scale;
    }

    private static Vector3 BoundsToLocalScale(Transform transform, Vector3 worldSize)
    {
        Vector3 lossy = transform.lossyScale;
        return new Vector3(
            worldSize.x / Mathf.Max(Mathf.Abs(lossy.x), 1e-4f),
            worldSize.y / Mathf.Max(Mathf.Abs(lossy.y), 1e-4f),
            worldSize.z / Mathf.Max(Mathf.Abs(lossy.z), 1e-4f));
    }

    private static void ApplyCapsuleTransform(Transform reveal, CapsuleCollider capsule)
    {
        float diameter = capsule.radius * 2f;
        float height = Mathf.Max(capsule.height, diameter);

        reveal.localPosition = capsule.center;
        // Primitive capsule mesh is Y-up (height 2, diameter 1); rotate to match CapsuleCollider.direction.
        reveal.localRotation = capsule.direction switch
        {
            0 => Quaternion.Euler(0f, 0f, 90f),
            2 => Quaternion.Euler(90f, 0f, 0f),
            _ => Quaternion.identity,
        };
        reveal.localScale = new Vector3(diameter, height * 0.5f, diameter);
    }
}
