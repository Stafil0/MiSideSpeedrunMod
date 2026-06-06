using SpeedrunMod.Notifications;
using SpeedrunMod.RevealSystems.Colliders;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class RevealColliderToggle
{
    internal static void Update()
    {
        if (!Input.GetKey(KeyCode.LeftAlt) || !Input.GetKeyDown(KeyCode.H))
        {
            return;
        }

        var mode = Colliders.Switch();
        var message = DescribeMode(mode);

        NotificationManager.Show(new NotificationMessage(message));
        Plugin.Log.LogInfo(message);
    }

    private static string DescribeMode(Colliders.RevealMode mode) => mode switch
    {
        Colliders.RevealMode.Primitives => "Physics colliders: primitives only",
        Colliders.RevealMode.Mesh => "Physics colliders: mesh only",
        Colliders.RevealMode.All => "Physics colliders: all",
        _ => "Physics colliders: off"
    };
}
