using System;
using System.Collections.Generic;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems.Colliders;

internal static class Colliders
{
    private const float RescanIntervalSeconds = 2f;

    private static readonly Dictionary<int, ColliderEntry> Entries = new();
    private static bool _isRevealing;
    private static float _nextRescanAt;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int Mode = Shader.PropertyToID("_Mode");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");

    internal static bool IsRevealing() => _isRevealing;

    internal static void Reveal()
    {
        Clear();
        _isRevealing = true;
        _nextRescanAt = Time.realtimeSinceStartup + RescanIntervalSeconds;

        int count = ScanColliders();
        Plugin.Log.LogInfo($"[Colliders] Reveal drew {count} colliders ({Entries.Count} entries)");
    }

    internal static void Hide()
    {
        Clear();
        _isRevealing = false;
    }

    internal static void Clear()
    {
        int count = Entries.Count;

        foreach (var (_, entry) in Entries)
        {
            DestroyEntry(entry);
        }

        ObjectRegistry.Clear(Entries.Keys);
        Entries.Clear();

        if (count > 0)
        {
            Plugin.Log.LogInfo($"[Colliders] Clear removed {count} entries");
        }
    }

    internal static void Update()
    {
        if (!_isRevealing)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (now >= _nextRescanAt)
        {
            _nextRescanAt = now + RescanIntervalSeconds;
            ScanColliders();
            ClearDestroyedEntries();
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

    private static int ScanColliders()
    {
        int count = 0;

        count += ProcessColliders<BoxCollider>("box");
        count += ProcessColliders<SphereCollider>("sphere");
        count += ProcessColliders<CapsuleCollider>("capsule");

        return count;
    }

    private static int ProcessColliders<T>(string shapeType) where T : Collider
    {
        PlayerMove playerMove = PlayerUtil.ResolvePlayerMove();
        T[] colliders = UnityEngine.Object.FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        int count = 0;

        foreach (T collider in colliders)
        {
            if (!ShouldProcess(collider))
            {
                continue;
            }

            string resolvedType = ResolveType(collider, playerMove, shapeType);

            AddRevealer(
                collider,
                resolvedType,
                syncTransform: resolvedType == "player",
                labelText: $"{typeof(T).Name} : {collider.gameObject.name}");

            count++;
        }

        return count;
    }

    private static string ResolveType(Collider collider, PlayerMove playerMove, string shapeType)
    {
        if (PlayerUtil.IsOnPlayerHierarchy(collider, playerMove))
        {
            return "player";
        }

        if (collider.isTrigger)
        {
            return "trigger";
        }

        return shapeType;
    }

    private static bool ShouldProcess(Collider collider)
    {
        if (collider == null || !collider.enabled)
        {
            return false;
        }

        if (collider is MeshCollider { convex: false })
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

        if (InteractiveUtil.IsInteractive(collider))
        {
            return false;
        }

        if (ComponentUtil.IsTrigger(collider.gameObject))
        {
            return false;
        }

        return true;
    }

    private static void AddRevealer(
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
                GetColor(type));
        }

        var entry = new ColliderEntry(collider, type, syncTransform)
        {
            Shape = shape,
            HasShape = hasShape,
            Label = labelUi
        };

        ApplyReveal(entry);
        Entries[collider.GetInstanceID()] = entry;
    }

    private static void DestroyEntry(ColliderEntry entry)
    {
        if (entry.Shape != null)
        {
            ObjectRegistry.Destroy(entry.Shape);
            entry.Shape = null;
            entry.HasShape = false;
        }

        if (entry.Label != null)
        {
            ObjectRegistry.Destroy(entry.Label.transform.parent.gameObject);
            ObjectRegistry.Destroy(entry.Label.gameObject);
            entry.Label = null;
        }
    }

    private static void ClearDestroyedEntries()
    {
        foreach (var (id, entry) in Entries)
        {
            if (entry.Source != null)
            {
                continue;
            }

            DestroyEntry(entry);
            Entries.Remove(id);
        }
    }

    private static void ApplyReveal(ColliderEntry entry)
    {
        ApplyMaterial(entry.Shape, entry.Type);

        if (entry.Label == null)
        {
            return;
        }

        Color labelColor = GetColor(entry.Type);
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
        Color baseColor = GetColor(type);
        mat.SetColor(ColorId, baseColor);
        mat.SetColor("_EmissionColor", baseColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetFloat(Mode, 3);
        mat.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        meshRenderer.material = mat;
    }

    private static Color GetColor(string type) => type switch
    {
        "player" => new Color(0.1f, 0.95f, 1f, 0.35f),
        "box" => new Color(1f, 0.55f, 0.1f, 0.25f),
        "sphere" => new Color(0.9f, 0.4f, 1f, 0.25f),
        "capsule" => new Color(1f, 0.85f, 0.2f, 0.25f),
        "trigger" => new Color(0.2f, 1f, 0.45f, 0.3f),
        _ => new Color(1f, 1f, 1f, 0.2f)
    };
}
