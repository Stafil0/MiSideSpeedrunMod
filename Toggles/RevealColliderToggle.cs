using SpeedrunMod.Notifications;
using SpeedrunMod.RevealSystems.Colliders;

namespace SpeedrunMod.Toggles;

internal static class RevealColliderToggle
{
    internal static void Update()
    {
        if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.H)) return;

        if (Colliders.IsRevealing())
        {
            NotificationManager.Show(new NotificationMessage("Collider reveal off"));
            Plugin.Log.LogInfo("Collider reveal off");
            Colliders.HideColliders();
        }
        else
        {
            NotificationManager.Show(new NotificationMessage("Collider reveal on"));
            Plugin.Log.LogInfo("Collider reveal on");
            Colliders.RevealColliders();
        }
    }
}
