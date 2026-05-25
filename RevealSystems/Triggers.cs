using System.Collections.Generic;
using System.Linq;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems;

internal static class Triggers
{
    private static readonly Dictionary<int, TriggerEntry> Entries = new();
    private static bool _isRevealing;
    private static readonly int Color = Shader.PropertyToID("_Color");
    private static readonly int Mode = Shader.PropertyToID("_Mode");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private const string FallbackShapePrefix = "RevealDefault_";

    internal static void CacheEventCollider(Trigger_Event trigger, BoxCollider box)
    {
        int id = trigger.GetInstanceID();
        Vector3 half = box.size * 0.5f;
        Vector3? colliderSize = half.sqrMagnitude > 0f ? half : null;

        if (Entries.TryGetValue(id, out TriggerEntry entry))
        {
            if (colliderSize.HasValue)
            {
                entry.ColliderSize = colliderSize;
                entry.ColliderCenter = box.center;
                Plugin.Log.LogInfo($"[Triggers] Cached event box half={half} center={box.center} on {entry.Name} (existing entry)");
            }

            return;
        }

        entry = new TriggerEntry(trigger, "event", colliderSize: colliderSize, colliderCenter: colliderSize.HasValue ? box.center : null);
        Entries[id] = entry;

        Plugin.Log.LogInfo(colliderSize.HasValue
            ? $"[Triggers] Cached event box half={half} center={box.center} on {entry.Name} (new stub)"
            : $"[Triggers] Cached event stub on {entry.Name} (no collider size, default cube on reveal)");
    }

    internal static void ClearEntries()
    {
        int count = Entries.Count;
        Entries.Clear();
        Plugin.Log.LogInfo($"[Triggers] ClearEntries removed {count} entries");
    }

    internal static void RevealTriggers()
    {
        HideTriggers();
        _isRevealing = true;

        int count = 0;
        count += ProcessTriggers<Trigger_DistanceCamera>("distancecamera");
        count += ProcessTriggers<Trigger_DistanceCheck>("distancecheck");
        count += ProcessTriggers<Trigger_DistanceCircle>("distancecircle");
        count += ProcessTriggers<Trigger_Event>("event");
        count += ProcessTriggers<Trigger_MouseClick>("mouseclick");
        count += ProcessTriggers<Trigger_MouseEvent>("mouseevent");
        count += ProcessTriggers<Trigger_Teleport>("teleport");
        count += ProcessTriggers<Trigger_Zoom>("zoom");

        Plugin.Log.LogInfo($"[Triggers] RevealTriggers drew {count} triggers ({Entries.Count} entries in dictionary)");
    }

    private static int ProcessTriggers<T>(string type) where T : Component
    {
        var objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in objects)
        {
            AddTriggerRevealer(obj, type);
        }

        if (objects.Length > 0)
        {
            Plugin.Log.LogInfo($"[Triggers] ProcessTriggers<{typeof(T).Name}> type={type} count={objects.Length}");
        }

        return objects.Length;
    }

    private static void AddTriggerRevealer(Component trigger, string type)
    {
        GameObject gameObject = trigger.gameObject;
        bool hasShape = TryCreateRevealShape(trigger, out GameObject newObject);

        GameObject canvasGUI = CreateCanvas(gameObject);
        Text label = CreateTextUI(canvasGUI, type, gameObject.name);

        var revealCollider = newObject.GetComponent<Collider>();
        if (revealCollider != null)
        {
            revealCollider.enabled = false;
        }

        var id = trigger.GetInstanceID();
        var entry = new TriggerEntry(trigger, type);
        if (TryGetTriggerEventBox(trigger, out Vector3 halfExtents, out Vector3 localCenter))
        {
            entry.ColliderSize = halfExtents;
            entry.ColliderCenter = localCenter;
        }

        entry.Source = trigger;
        entry.Type = type;
        entry.Shape = newObject;
        entry.HasShape = hasShape;
        entry.Label = label;

        ApplyRevealStyle(entry);

        Entries[id] = entry;

        Plugin.Log.LogInfo(
            $"[Triggers] Reveal {entry.Name} type={type} hasShape={entry.HasShape} " +
            $"cachedHalf={(entry.ColliderSize.HasValue ? entry.ColliderSize.Value.ToString() : "none")} shape={newObject.name}");
    }

    private static bool TryCreateRevealShape(Component trigger, out GameObject shape)
    {
        shape = null;
        var name = trigger.GetName();

        if (TryCreateEventReveal(trigger, out shape))
        {
            return true;
        }

        Plugin.Log.LogWarning($"[Triggers] {name} could not create shape from event");

        if (TryCreateFromCollider(trigger, out shape))
        {
            return true;
        }

        Plugin.Log.LogWarning($"[Triggers] {name} could not create shape from collider");

        if (TryCreateFromTriggerLogic(trigger, out shape))
        {
            return true;
        }

        Plugin.Log.LogWarning($"[Triggers] {name} could not create shape from trigger logic");

        shape = CreateDefaultRevealCube(trigger);

        Plugin.Log.LogWarning($"[Triggers] {name} using 1x1 default cube");

        return false;
    }

    private static bool TryCreateEventReveal(Component trigger, out GameObject reveal)
    {
        reveal = null;
        if (!TryGetTriggerEventBox(trigger, out Vector3 halfExtents, out Vector3 localCenter))
        {
            return false;
        }

        reveal = CreateBoxVolume(trigger.transform, halfExtents * 2f, localCenter);
        return true;
    }

    private static bool TryCreateFromCollider(Component trigger, out GameObject reveal)
    {
        reveal = null;
        var name = trigger.GetName();
        Collider collider = FindBestCollider(trigger);
        if (collider == null)
        {
            Plugin.Log.LogDebug($"[Triggers] {name} volume: no collider");
            return false;
        }

        reveal = CreatePrimitiveForCollider(collider);
        if (reveal == null)
        {
            return false;
        }

        reveal.name = "Reveal_" + trigger.gameObject.name;
        ApplyColliderTransform(reveal.transform, collider);

        Plugin.Log.LogDebug(
            $"[Triggers] {name} volume=collider {collider.GetName()} " +
            $"isTrigger={collider.isTrigger} scale={reveal.transform.localScale}");
        
        return true;
    }

    private static Collider FindBestCollider(Component trigger)
    {
        if (trigger is Trigger_Event)
        {
            return null;
        }

        Collider[] colliders = trigger.GetComponentsInChildren<Collider>(true);
        Collider best = null;
        float bestScore = 0f;

        foreach (Collider collider in colliders)
        {
            if (collider is MeshCollider { convex: false })
            {
                continue;
            }

            float score = collider.bounds.size.sqrMagnitude;
            if (score > bestScore)
            {
                bestScore = score;
                best = collider;
            }
        }

        return best;
    }

    private static GameObject CreatePrimitiveForCollider(Collider collider)
    {
        return collider switch
        {
            BoxCollider => GameObject.CreatePrimitive(PrimitiveType.Cube),
            SphereCollider => GameObject.CreatePrimitive(PrimitiveType.Sphere),
            CapsuleCollider => GameObject.CreatePrimitive(PrimitiveType.Capsule),
            _ => GameObject.CreatePrimitive(PrimitiveType.Cube)
        };
    }

    private static void ApplyColliderTransform(Transform reveal, Collider collider)
    {
        reveal.SetParent(collider.transform, false);
        reveal.localRotation = Quaternion.identity;

        switch (collider)
        {
            case BoxCollider box:
                reveal.localPosition = box.center;
                reveal.localScale = box.size;
                break;

            case SphereCollider sphere:
                reveal.localPosition = sphere.center;
                reveal.localScale = Vector3.one * (sphere.radius * 2f);
                break;

            case CapsuleCollider capsule:
                reveal.localPosition = capsule.center;
                ApplyCapsuleScale(reveal, capsule);
                break;

            default:
                Bounds bounds = collider.bounds;
                reveal.SetParent(collider.transform.parent != null ? collider.transform.parent : collider.transform, true);
                reveal.position = bounds.center;
                reveal.rotation = collider.transform.rotation;
                reveal.localScale = bounds.size;
                break;
        }
    }

    private static void ApplyCapsuleScale(Transform reveal, CapsuleCollider capsule)
    {
        // Unity capsule primitive: height 2 (Y), radius 0.5 on X/Z for direction Y.
        float diameter = capsule.radius * 2f;
        float height = Mathf.Max(capsule.height, diameter);

        switch (capsule.direction)
        {
            case 0: // X
                reveal.localScale = new Vector3(height * 0.5f, diameter * 0.5f, diameter * 0.5f);
                break;
            case 2: // Z
                reveal.localScale = new Vector3(diameter * 0.5f, diameter * 0.5f, height * 0.5f);
                break;
            default: // Y
                reveal.localScale = new Vector3(diameter * 0.5f, height * 0.5f, diameter * 0.5f);
                break;
        }
    }

    private static bool TryCreateFromTriggerLogic(Component trigger, out GameObject reveal)
    {
        reveal = null;
        var name = trigger.GetName();

        switch (trigger)
        {
            case Trigger_DistanceCircle circle:
                reveal = CreateSphereVolume(circle.transform, circle.radius, Vector3.zero);
                Plugin.Log.LogDebug($"[Triggers] {name} volume=distancecircle radius={circle.radius}");
                return true;

            case Trigger_DistanceCamera cameraTrigger:
                reveal = CreateSphereVolume(cameraTrigger.transform, cameraTrigger.distance, Vector3.zero);
                Plugin.Log.LogDebug($"[Triggers] {name} volume=distancecamera distance={cameraTrigger.distance}");
                return true;

            case Trigger_Zoom zoom:
                reveal = CreateSphereVolume(zoom.transform, zoom.distance, Vector3.zero);
                Plugin.Log.LogDebug($"[Triggers] {name} volume=zoom distance={zoom.distance}");
                return true;

            default:
                return false;
        }
    }

    private static bool TryGetTriggerEventBox(Component trigger, out Vector3 halfExtents, out Vector3 localCenter)
    {
        if (trigger is not Trigger_Event triggerEvent)
        {
            halfExtents = Vector3.zero;
            localCenter = Vector3.zero;
            return false;
        }

        return TryGetTriggerEventBox(triggerEvent, out halfExtents, out localCenter);
    }

    private static bool TryGetTriggerEventBox(Trigger_Event triggerEvent, out Vector3 halfExtents, out Vector3 localCenter)
    {
        int id = triggerEvent.GetInstanceID();

        if (Entries.TryGetValue(id, out TriggerEntry entry))
        {
            if (entry.ColliderSize.HasValue && entry.ColliderCenter.HasValue)
            {
                halfExtents = entry.ColliderSize.Value;
                localCenter = entry.ColliderCenter.Value;
                return true;
            }
        }

        BoxCollider box = triggerEvent.GetComponent<BoxCollider>();
        if (box != null)
        {
            halfExtents = box.size * 0.5f;
            localCenter = box.center;
            return halfExtents.sqrMagnitude > 0f;
        }

        halfExtents = Vector3.zero;
        localCenter = Vector3.zero;
        return false;
    }

    private static GameObject CreateBoxVolume(Transform parent, Vector3 size, Vector3 localCenter)
    {
        GameObject reveal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        reveal.name = "Reveal_" + parent.name;
        reveal.transform.SetParent(parent, false);
        reveal.transform.localPosition = localCenter;
        reveal.transform.localRotation = Quaternion.identity;
        reveal.transform.localScale = size;
        return reveal;
    }

    private static GameObject CreateSphereVolume(Transform parent, float radius, Vector3 localCenter)
    {
        GameObject reveal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        reveal.name = "Reveal_" + parent.name;
        reveal.transform.SetParent(parent, false);
        reveal.transform.localPosition = localCenter;
        reveal.transform.localRotation = Quaternion.identity;
        float diameter = Mathf.Max(radius * 2f, 0.05f);
        reveal.transform.localScale = new Vector3(diameter, diameter, diameter);
        return reveal;
    }

    private static GameObject CreateDefaultRevealCube(Component trigger)
    {
        var transform = trigger.transform;
        GameObject reveal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        reveal.name = FallbackShapePrefix + transform.name;
        reveal.transform.SetParent(transform, false);
        reveal.transform.localPosition = Vector3.zero;
        reveal.transform.localRotation = Quaternion.identity;
        reveal.transform.localScale = Vector3.one;
        return reveal;
    }

    private static GameObject CreateCanvas(GameObject parent)
    {
        GameObject canvasGUI = new GameObject("Canvas " + parent.name);
        canvasGUI.AddComponent<Canvas>();
        canvasGUI.AddComponent<CanvasScaler>();
        canvasGUI.AddComponent<GraphicRaycaster>();
        canvasGUI.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        canvasGUI.transform.SetParent(parent.transform, false);
        canvasGUI.transform.localPosition = Vector3.up * .5f;
        canvasGUI.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f);
        return canvasGUI;
    }

    private static Text CreateTextUI(GameObject parent, string type, string name)
    {
        GameObject textGUI = new GameObject("Text " + parent.name);
        textGUI.transform.SetParent(parent.transform, false);
        Text text = textGUI.AddComponent<Text>();
        text.text = type + " : " + name;
        text.fontSize = 30;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;

        text.rectTransform.localPosition = Vector3.zero;
        text.rectTransform.localScale = new Vector3(0.3f, 0.3f, 1);
        text.rectTransform.localRotation = Quaternion.identity;
        return text;
    }

    private static void ApplyRevealStyle(TriggerEntry entry)
    {
        ApplyMaterial(entry.Shape, entry.Type);

        if (entry.Label != null && entry.Source != null)
        {
            Color labelColor = GetColorForTrigger(entry.Type);
            entry.Label.color = new Color(labelColor.r, labelColor.g, labelColor.b, 1f);
            string objectName = entry.Source.gameObject.name;
            if (!entry.HasShape)
            {
                objectName += " [default cube]";
            }

            entry.Label.text = entry.Type + " : " + objectName;
        }
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
        Color baseColor = GetColorForTrigger(type);
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

    private static Color GetColorForTrigger(string type) => type switch
    {
        "distancecamera" => new Color(0.98f, 0.0f, 0.0f, .2f),
        "distancecheck" => new Color(1f, 0.4f, 0.0f, .2f),
        "distancecircle" => new Color(0.0f, 0.2f, 0.98f, .2f),
        "event" => new Color(0.0f, 0.97f, 0.0f, .2f),
        "mouseclick" => new Color(0.3f, 1f, 1f, .2f),
        "mouseevent" => new Color(0.9f, 0.9f, 0.0f, .2f),
        "teleport" => new Color(0.9f, 0.2f, 0.7f, .2f),
        "zoom" => new Color(0.8f, 0.8f, 0.8f, .2f),
        _ => new Color(1f, 1f, 1f, .2f)
    };

    internal static void HideTriggers()
    {
        _isRevealing = false;
        int removed = 0;
        int kept = 0;
        foreach (var (id, entry) in Entries)
        {
            if (entry.Shape != null)
            {
                Object.Destroy(entry.Shape);
                entry.Shape = null;
                entry.HasShape = false;
            }

            if (entry.Label != null)
            {
                Object.Destroy(entry.Label.gameObject.transform.parent.gameObject);
                entry.Label = null;
            }

            if (!entry.ColliderSize.HasValue)
            {
                Entries.Remove(id);
                removed++;
            }
            else
            {
                kept++;
            }
        }

        Plugin.Log.LogInfo($"[Triggers] HideTriggers removed {removed} entries, kept {kept} stubs");
    }

    public static bool IsRevealing()
    {
        return _isRevealing;
    }

    internal static void Update()
    {
        if (!_isRevealing)
        {
            return;
        }

        Camera cam = Camera.main;
        foreach (TriggerEntry entry in Entries.Values)
        {
            if (entry.Source == null || entry.Shape == null)
            {
                continue;
            }

            if (entry.Label != null && cam != null)
            {
                Transform canvas = entry.Label.transform.parent;
                if (canvas != null)
                {
                    canvas.LookAt(2f * canvas.position - cam.transform.position, cam.transform.up);
                }
            }
        }
    }
}
