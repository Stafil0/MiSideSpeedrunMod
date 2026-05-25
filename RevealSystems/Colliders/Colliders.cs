using System;
using System.Collections.Generic;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems.Colliders;

internal static class Colliders
{
    private static readonly Dictionary<int, ColliderEntry> Entries = new();
    private static bool _isRevealing;

    private static readonly int Color = Shader.PropertyToID("_Color");
    private static readonly int Mode = Shader.PropertyToID("_Mode");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");

    private static readonly (string Type, Func<int> Process)[] RevealSources =
    {
        ("box", () => ProcessColliders<BoxCollider>("box")),
        ("sphere", () => ProcessColliders<SphereCollider>("sphere")),
        ("capsule", () => ProcessColliders<CapsuleCollider>("capsule")),
    };

    internal static void RevealColliders()
    {
        HideColliders();
        _isRevealing = true;

        int count = 0;
        foreach (var (_, process) in RevealSources)
        {
            count += process();
        }

        Plugin.Log.LogInfo($"[Colliders] RevealColliders drew {count} colliders ({Entries.Count} entries)");
    }

    internal static void HideColliders()
    {
        _isRevealing = false;

        foreach (ColliderEntry entry in Entries.Values)
        {
            if (entry.Shape != null)
            {
                ObjectRegistry.Destroy(entry.Shape);
            }

            if (entry.Label != null)
            {
                ObjectRegistry.Destroy(entry.Label.transform.parent.gameObject);
            }
        }

        int count = Entries.Count;
        ObjectRegistry.Clear(Entries.Keys);
        Entries.Clear();

        Plugin.Log.LogInfo($"[Colliders] HideColliders removed {count} entries");
    }

    internal static bool IsRevealing() => _isRevealing;

    internal static void ClearEntries()
    {
        ObjectRegistry.Clear(Entries.Keys);
        Entries.Clear();
    }

    internal static void Update()
    {
        if (!_isRevealing)
        {
            return;
        }

        Camera cam = Camera.main;

        foreach (ColliderEntry entry in Entries.Values)
        {
            if (entry.Source == null)
            {
                continue;
            }

            if (entry.InSync && entry.Shape != null)
            {
                ColliderUtil.ApplyTransform(entry.Shape.transform, entry.Source);
            }

            if (entry.Label == null || cam == null)
            {
                continue;
            }

            Transform canvas = entry.Label.transform.parent;
            if (canvas != null)
            {
                canvas.localPosition = ColliderUtil.ResolveLabelPosition(entry.Source);
                canvas.LookAt(2f * canvas.position - cam.transform.position, cam.transform.up);
            }
        }
    }

    private static int ProcessColliders<T>(string type) where T : Collider
    {
        PlayerMove playerMove = PlayerUtil.ResolvePlayerMove();
        T[] colliders = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;

        foreach (T collider in colliders)
        {
            if (!ShouldReveal(collider))
            {
                continue;
            }

            string resolvedType = ResolveType(collider, playerMove, type);

            AddColliderReveal(
                collider,
                resolvedType,
                syncTransform: resolvedType == "player",
                labelText: $"{typeof(T).Name} : {collider.gameObject.name}");
            
            count++;
        }

        if (colliders.Length > 0)
        {
            Plugin.Log.LogInfo($"[Colliders] ProcessColliders<{typeof(T).Name}> type={type} count={count}");
        }

        return count;
    }

    private static string ResolveType(Collider collider, PlayerMove playerMove, string defaultType)
    {
        bool onPlayer = PlayerUtil.IsOnPlayerHierarchy(collider, playerMove);
        bool interactable = InteractiveUtil.IsInteractable(collider);

        return onPlayer ? "player"
            : interactable ? "interactive"
            : collider.isTrigger ? "trigger"
            : defaultType;
    }

    private static bool ShouldReveal(Collider collider)
    {
        if (collider == null || !collider.enabled)
        {
            return false;
        }

        if (Entries.ContainsKey(collider.GetInstanceID()))
        {
            return false;
        }

        if (ObjectRegistry.IsInRegistry(collider.gameObject))
        {
            return false;
        }

        if (collider is MeshCollider { convex: false })
        {
            return false;
        }

        var gameObject = collider.gameObject;
        foreach (Component component in gameObject.GetComponentsInParent<Component>(true))
        {
            if (component.GetType().Name.StartsWith("Trigger_"))
            {
                return false;
            }
        }

        return true;
    }

    private static void AddColliderReveal(
        Collider collider,
        string type,
        bool syncTransform,
        string labelText = null)
    {
        GameObject shape = ColliderUtil.CreateShape(collider, $"Reveal_{type}_{collider.gameObject.name}");
        bool hasShape = shape != null;

        if (hasShape)
        {
            ObjectRegistry.Register(shape);
        }

        Text labelUi = null;
        if (!string.IsNullOrEmpty(labelText) && hasShape)
        {
            GameObject canvas = ObjectRegistry.CreateCanvas(
                collider.transform,
                ColliderUtil.ResolveLabelPosition(collider));
            
            labelUi = ObjectRegistry.CreateLabel(
                canvas.transform,
                labelText,
                GetColorForType(type));
        }

        var entry = new ColliderEntry(collider, type, syncTransform)
        {
            Shape = shape,
            HasShape = hasShape,
            Label = labelUi
        };

        ApplyRevealStyle(entry);
        Entries[collider.GetInstanceID()] = entry;

        Plugin.Log.LogInfo($"[Colliders] Reveal {entry.Name} type={type} hasShape={hasShape}");
    }

    private static void ApplyRevealStyle(ColliderEntry entry)
    {
        ApplyMaterial(entry.Shape, entry.Type);

        if (entry.Label == null)
        {
            return;
        }

        Color labelColor = GetColorForType(entry.Type);
        entry.Label.color = new Color(labelColor.r, labelColor.g, labelColor.b, 1f);
    }

    private static void ApplyMaterial(GameObject volume, string type)
    {
        if (volume == null)
        {
            return;
        }

        MeshRenderer meshRenderer = volume.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            return;
        }

        Material mat = new Material(Shader.Find("Standard"));
        Color baseColor = GetColorForType(type);
        mat.SetColor(Color, baseColor);
        mat.SetColor("_EmissionColor", baseColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetFloat(Mode, 3);
        mat.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        meshRenderer.material = mat;
    }

    private static Color GetColorForType(string type) => type switch
    {
        "player" => new Color(0.1f, 0.95f, 1f, 0.35f),
        "box" => new Color(1f, 0.55f, 0.1f, 0.25f),
        "sphere" => new Color(0.9f, 0.4f, 1f, 0.25f),
        "capsule" => new Color(1f, 0.85f, 0.2f, 0.25f),
        "trigger" => new Color(0.2f, 1f, 0.45f, 0.3f),
        "interactive" => new Color(1f, 0.25f, 0.85f, 0.35f),
        _ => new Color(1f, 1f, 1f, 0.2f)
    };
}
