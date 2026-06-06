using SpeedrunMod.Notifications;
using SpeedrunMod.RevealSystems.Visuals;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class HideTextureToggle
{
    internal static void Update()
    {
        if (!Input.GetKey(KeyCode.LeftAlt) || !Input.GetKeyDown(KeyCode.T))
        {
            return;
        }

        TextureRenderer.Scope scope = TextureRenderer.Switch();
        string message = DescribeScope(scope);

        NotificationManager.Show(new NotificationMessage(message));
        Plugin.Log.LogInfo(message);
    }

    private static string DescribeScope(TextureRenderer.Scope scope) => scope switch
    {
        TextureRenderer.Scope.PrimitiveColliders => "Textures hidden: primitive colliders",
        TextureRenderer.Scope.MeshColliders => "Textures hidden: mesh colliders",
        TextureRenderer.Scope.AllColliders => "Textures hidden: all colliders",
        TextureRenderer.Scope.AllRenderers => "Textures hidden: all",
        _ => "Textures restored"
    };
}
