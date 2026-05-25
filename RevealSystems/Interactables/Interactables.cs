using System.Collections.Generic;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems.Interactables;

internal static class Interactables
{
    private const float RescanIntervalSeconds = 2f;

    private static readonly Dictionary<int, InteractableEntry> Entries = new();
    private static bool _isRevealing;
    private static float _nextRescanAt;

    private static readonly Color OutOfRangeColor = new(0.95f, 0.15f, 1f, 0.5f);
    private static readonly Color NearbyColor = new(0.45f, 0.65f, 1f, 0.45f);
    private static readonly Color AimedColor = new(1f, 0.85f, 0.2f, 0.45f);
    private static readonly Color InRangeColor = new(0.25f, 1f, 0.55f, 0.55f);

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

        Plugin.Log.LogInfo($"[Interactables] Reveal drew {count} colliders ({Entries.Count} entries)");
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
            Plugin.Log.LogInfo($"[Interactables] Clear removed {count} entries");
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
        if (cam == null)
        {
            return;
        }

        PlayerMove playerMove = PlayerUtil.ResolvePlayerMove();

        foreach (InteractableEntry entry in Entries.Values)
        {
            if (entry.Source == null || entry.Label == null)
            {
                continue;
            }

            Transform canvas = entry.Label.transform.parent;
            if (canvas == null)
            {
                continue;
            }

            canvas.localPosition = ColliderUtil.ResolveLabelPosition(entry.Source);
            canvas.LookAt(2f * canvas.position - cam.transform.position, cam.transform.up);
        }

        UpdateInteractionRange(cam, playerMove);
    }

    private static int ScanColliders()
    {
        int count = 0;
        count += ProcessColliders<BoxCollider>();
        count += ProcessColliders<SphereCollider>();
        count += ProcessColliders<CapsuleCollider>();
        return count;
    }

    private static int ProcessColliders<T>() where T : Collider
    {
        T[] colliders = Object.FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        int count = 0;

        foreach (T collider in colliders)
        {
            if (!ShouldProcess(collider))
            {
                continue;
            }

            AddRevealer(collider, $"{typeof(T).Name} : {collider.gameObject.name}");
            count++;
        }

        return count;
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

        Plugin.Log.LogInfo($"[Interactables] ClearDestroyedEntries removed {removed} entries");
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

        if (!InteractiveUtil.IsInteractive(collider))
        {
            return false;
        }

        if (ComponentUtil.IsTrigger(collider.gameObject))
        {
            return false;
        }

        return true;
    }

    private static void AddRevealer(Collider collider, string labelText)
    {
        GameObject shape = ColliderUtil.CreateShape(collider, $"Reveal_interactable_{collider.gameObject.name}");
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

            labelUi = ObjectRegistry.CreateLabel(canvas.transform, labelText, OutOfRangeColor);
        }

        var entry = new InteractableEntry(collider)
        {
            Shape = shape,
            HasShape = hasShape,
            Label = labelUi,
            Text = labelText
        };

        ApplyMaterial(entry, OutOfRangeColor);
        Entries[collider.GetInstanceID()] = entry;
    }

    private static void DestroyEntry(InteractableEntry entry)
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

    private static void UpdateInteractionRange(Camera camera, PlayerMove playerMove)
    {
        foreach (InteractableEntry entry in Entries.Values)
        {
            if (entry.Source == null)
            {
                continue;
            }

            InteractableState state = ResolveRangeState(camera, entry.Source, playerMove);

            if (entry.State != state)
            {
                entry.State = state;
                ApplyMaterial(entry, ResolveRangeColor(state));
            }

            if (entry.Label == null || string.IsNullOrEmpty(entry.Text))
            {
                continue;
            }

            if (!InteractiveUtil.TryResolveDistance(playerMove, entry.Source, out float distance))
            {
                continue;
            }

            string labelText = $"{entry.Text} {distance:F1}m";
            if (entry.Label.text != labelText)
            {
                entry.Label.text = labelText;
            }
        }
    }

    private static InteractableState ResolveRangeState(Camera camera, Collider collider, PlayerMove playerMove)
    {
        if (InteractiveUtil.IsInRange(collider, playerMove))
        {
            return InteractableState.InRange;
        }

        if (InteractiveUtil.IsAimedAt(camera, collider, playerMove))
        {
            return InteractableState.Aimed;
        }

        if (InteractiveUtil.IsWithinReach(collider, playerMove))
        {
            return InteractableState.Nearby;
        }

        return InteractableState.OutOfRange;
    }

    private static Color ResolveRangeColor(InteractableState rangeState) => rangeState switch
    {
        InteractableState.InRange => InRangeColor,
        InteractableState.Aimed => AimedColor,
        InteractableState.Nearby => NearbyColor,
        _ => OutOfRangeColor,
    };

    private static void ApplyMaterial(InteractableEntry entry, Color color)
    {
        if (entry.Shape == null)
        {
            return;
        }

        MeshRenderer meshRenderer = entry.Shape.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            return;
        }

        meshRenderer.material = ApplyMaterial(color);

        if (entry.Label != null)
        {
            entry.Label.color = new Color(color.r, color.g, color.b, 1f);
        }
    }

    private static void ApplyMaterial(MeshRenderer meshRenderer, Color color)
    {
        if (meshRenderer == null)
        {
            return;
        }

        meshRenderer.material = ApplyMaterial(color);
    }

    private static Material ApplyMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetColor(ColorId, color);
        mat.SetColor("_EmissionColor", color);
        mat.EnableKeyword("_EMISSION");
        mat.SetFloat(Mode, 3);
        mat.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3100;
        return mat;
    }
}
