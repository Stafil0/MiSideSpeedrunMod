using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems;

internal static class ObjectRegistry
{
    private static readonly HashSet<int> Ids = new();

    internal static GameObject Register(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Ids.Add(gameObject.GetInstanceID());
        }

        return gameObject;
    }

    internal static void Unregister(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Ids.Remove(gameObject.GetInstanceID());
        }
    }

    internal static void Clear()
    {
        Ids.Clear();
    }

    internal static void Clear(IEnumerable<int> ids)
    {
        foreach (int id in ids)
        {
            Ids.Remove(id);
        }
    }

    internal static bool IsInRegistry(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        for (Transform transform = gameObject.transform; transform != null; transform = transform.parent)
        {
            if (Ids.Contains(transform.gameObject.GetInstanceID()))
            {
                return true;
            }
        }

        return false;
    }

    internal static void Destroy(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        Unregister(gameObject);
        Object.Destroy(gameObject);
    }

    internal static GameObject CreatePrimitive(PrimitiveType type, string displayName)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
        gameObject.name = displayName;
        return Register(gameObject);
    }

    internal static GameObject CreateCanvas(Transform parent, float heightOffset = 0.5f)
    {
        return CreateCanvas(parent, Vector3.up * heightOffset);
    }

    internal static GameObject CreateCanvas(Transform parent, Vector3 localPosition)
    {
        GameObject canvas = new GameObject($"ModCanvas_{parent.name}");
        canvas.AddComponent<Canvas>();
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        canvas.transform.SetParent(parent, false);
        canvas.transform.localPosition = localPosition;
        canvas.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f);
        return Register(canvas);
    }

    internal static Text CreateLabel(Transform parent, string text, Color color)
    {
        GameObject textObject = new GameObject($"ModLabel_{parent.name}");
        textObject.transform.SetParent(parent, false);
        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.fontSize = 30;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.rectTransform.localPosition = Vector3.zero;
        label.rectTransform.localScale = new Vector3(0.3f, 0.3f, 1);
        label.rectTransform.localRotation = Quaternion.identity;
        Register(textObject);
        return label;
    }
}
