using System;
using System.Collections.Generic;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems.Colliders;

internal static class Colliders
{
    internal enum RevealMode
    {
        Off,
        Primitives,
        Mesh,
        All
    }

    private const float RescanIntervalSeconds = 2f;

    private static readonly Dictionary<int, ColliderEntry> Entries = new();
    private static RevealMode _mode = RevealMode.Off;
    private static float _nextRescanAt;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int Mode = Shader.PropertyToID("_Mode");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int ZTest = Shader.PropertyToID("_ZTest");
    private static readonly int Cull = Shader.PropertyToID("_Cull");

    internal static bool IsRevealing() => _mode != RevealMode.Off;

    internal static RevealMode Switch()
    {
        _mode = _mode switch
        {
            RevealMode.Off => RevealMode.Primitives,
            RevealMode.Primitives => RevealMode.Mesh,
            RevealMode.Mesh => RevealMode.All,
            _ => RevealMode.Off
        };

        Reveal();
        return _mode;
    }

    internal static void Reveal()
    {
        Clear();

        if (_mode == RevealMode.Off)
        {
            return;
        }

        _nextRescanAt = Time.realtimeSinceStartup + RescanIntervalSeconds;

        int count = ScanColliders();
        Plugin.Log.LogInfo($"[Colliders] Reveal mode={_mode}, drew {count} colliders ({Entries.Count} entries)");
    }

    internal static void Hide()
    {
        Clear();
        _mode = RevealMode.Off;
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
            Plugin.Log.LogDebug($"[Colliders] Clear removed {count} entries");
        }
    }

    internal static void Update()
    {
        if (_mode == RevealMode.Off)
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

        if (_mode == RevealMode.All || _mode == RevealMode.Primitives)
        {
            count += ProcessColliders<BoxCollider>("box");
            count += ProcessColliders<SphereCollider>("sphere");
            count += ProcessColliders<CapsuleCollider>("capsule");
        }

        if (_mode == RevealMode.All || _mode == RevealMode.Mesh)
        {
            count += ProcessColliders<MeshCollider>("mesh");
        }

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
            string labelText = $"{typeof(T).Name} : {collider.gameObject.name}";

            AddRevealer(
                collider,
                resolvedType,
                syncTransform: resolvedType == "player",
                labelText: labelText);

            count++;
        }

        if (count > 0)
        {
            Plugin.Log.LogDebug($"[Colliders] scan added {count} {shapeType} colliders of type {typeof(T).Name}");
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
        RegisterEntry(entry);
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

    private static void RegisterEntry(ColliderEntry entry)
    {
        if (entry.Shape != null)
        {
            ObjectRegistry.Register(entry.Shape);
        }

        if (entry.Label != null)
        {
            ObjectRegistry.Register(entry.Label.transform.parent.gameObject);
            ObjectRegistry.Register(entry.Label.gameObject);
        }
    }

    private static void ClearDestroyedEntries()
    {
        var removed = 0;

        foreach (var (id, entry) in Entries)
        {
            if (entry.Source != null)
            {
                continue;
            }

            DestroyEntry(entry);
            Entries.Remove(id);

            removed++;
        }

        if (removed > 0)
        {
            Plugin.Log.LogDebug($"[Colliders] ClearDestroyedEntries removed {removed} entries");
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
        mat.SetInt("_ZWrite", 0);
        mat.SetInt(ZTest, (int)UnityEngine.Rendering.CompareFunction.Always);
        mat.SetInt(Cull, (int)UnityEngine.Rendering.CullMode.Off);
        mat.renderQueue = 3100;

        var subMeshCount = 1;
        MeshFilter meshFilter = volume.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            subMeshCount = Mathf.Max(1, meshFilter.sharedMesh.subMeshCount);
        }

        if (subMeshCount <= 1)
        {
            meshRenderer.material = mat;
            return;
        }

        var mats = new Material[subMeshCount];
        for (int i = 0; i < subMeshCount; i++)
        {
            mats[i] = mat;
        }

        meshRenderer.materials = mats;
    }

    private static Color GetColor(string type) => type switch
    {
        "player" => new Color(0.1f, 0.95f, 1f, 0.35f),
        "box" => new Color(1f, 0.55f, 0.1f, 0.25f),
        "sphere" => new Color(0.9f, 0.4f, 1f, 0.25f),
        "capsule" => new Color(1f, 0.85f, 0.2f, 0.25f),
        "trigger" => new Color(0.2f, 1f, 0.45f, 0.3f),
        "mesh" => new Color(0.45f, 0.7f, 1f, 0.28f),
        _ => new Color(1f, 1f, 1f, 0.2f)
    };
}
