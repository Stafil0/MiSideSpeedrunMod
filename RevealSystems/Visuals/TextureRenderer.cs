using System.Collections.Generic;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.RevealSystems.Visuals;

internal static class TextureRenderer
{
    internal enum Scope
    {
        None,
        PrimitiveColliders,
        MeshColliders,
        AllColliders,
        AllRenderers
    }

    private const float RescanIntervalSeconds = 2f;

    private static readonly Dictionary<int, (Renderer Renderer, bool Enabled)> Hidden = new();
    private static Scope _scope = Scope.None;
    private static float _nextRescanAt;

    internal static bool IsActive() => _scope != Scope.None;

    internal static Scope Switch()
    {
        var scope = _scope switch
        {
            Scope.None => Scope.PrimitiveColliders,
            Scope.PrimitiveColliders => Scope.MeshColliders,
            Scope.MeshColliders => Scope.AllColliders,
            Scope.AllColliders => Scope.AllRenderers,
            _ => Scope.None
        };

        Enable(scope);
        return scope;
    }

    internal static void Enable() => Enable(_scope);
    
    private static void Enable(Scope scope)
    {
        Restore();

        _scope = scope;
        
        if (_scope == Scope.None)
        {
            return;
        }

        _nextRescanAt = Time.realtimeSinceStartup + RescanIntervalSeconds;
        int count = Scan();
        Plugin.Log.LogInfo($"[TextureRenderer] scope={_scope}, hid {count} renderer(s)");
    }

    internal static void Disable()
    {
        Restore();
        _scope = Scope.None;
    }

    internal static void Update()
    {
        if (_scope == Scope.None)
        {
            return;
        }

        if (Time.realtimeSinceStartup < _nextRescanAt)
        {
            return;
        }

        _nextRescanAt = Time.realtimeSinceStartup + RescanIntervalSeconds;
        Scan();
    }

    private static int Scan()
    {
        PlayerMove playerMove = PlayerUtil.ResolvePlayerMove();

        if (_scope == Scope.AllRenderers)
        {
            return ScanComponents<Renderer>(playerMove);
        }

        int count = 0;

        if (_scope == Scope.AllColliders || _scope == Scope.PrimitiveColliders)
        {
            count += ScanComponents<BoxCollider>(playerMove);
            count += ScanComponents<SphereCollider>(playerMove);
            count += ScanComponents<CapsuleCollider>(playerMove);
        }

        if (_scope == Scope.AllColliders || _scope == Scope.MeshColliders)
        {
            count += ScanComponents<MeshCollider>(playerMove);
        }

        return count;
    }

    private static int ScanComponents<T>(PlayerMove playerMove) where T : Component
    {
        T[] components = Object.FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        int count = 0;

        foreach (T component in components)
        {
            if (!ShouldHide(component, playerMove))
            {
                continue;
            }

            if (Hide(component))
            {
                count++;
            }
        }

        return count;
    }

    private static bool ShouldHide(Component component, PlayerMove playerMove) => component switch
    {
        Collider collider => ShouldHide(collider, playerMove),
        Renderer renderer => ShouldHide(renderer, playerMove),
        _ => false
    };

    private static bool Hide(Component component) => component switch
    {
        Collider collider => HideRenderer(collider.gameObject),
        Renderer renderer => HideRenderer(renderer),
        _ => false
    };

    private static bool ShouldHide(Renderer renderer, PlayerMove playerMove)
    {
        if (renderer == null)
        {
            return false;
        }

        if (ObjectRegistry.IsInRegistry(renderer.gameObject))
        {
            return false;
        }

        if (playerMove != null && renderer.transform.IsChildOf(playerMove.transform))
        {
            return false;
        }

        return true;
    }

    private static bool ShouldHide(Collider collider, PlayerMove playerMove)
    {
        if (collider == null || !collider.enabled || collider.isTrigger)
        {
            return false;
        }

        if (PlayerUtil.IsOnPlayerHierarchy(collider, playerMove))
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

    private static bool HideRenderer(GameObject source)
    {
        if (source == null)
        {
            return false;
        }

        bool hidAny = false;

        foreach (Renderer renderer in source.GetComponents<Renderer>())
        {
            if (HideRenderer(renderer))
            {
                hidAny = true;
            }
        }

        return hidAny;
    }

    private static bool HideRenderer(Renderer renderer)
    {
        if (renderer == null || ObjectRegistry.IsInRegistry(renderer.gameObject))
        {
            return false;
        }

        if (!Hidden.TryAdd(renderer.GetInstanceID(), (renderer, renderer.enabled)))
        {
            return false;
        }

        renderer.enabled = false;
        return true;
    }

    private static void Restore()
    {
        foreach (var (_, entry) in Hidden)
        {
            if (entry.Renderer != null)
            {
                entry.Renderer.enabled = entry.Enabled;
            }
        }

        Hidden.Clear();
    }
}
