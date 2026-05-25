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

        if (Colliders.IsRevealing())
        {
            NotificationManager.Show(new NotificationMessage("Physics colliders off"));
            Plugin.Log.LogInfo("Physics colliders turned off");
            Colliders.Hide();
        }
        else
        {
            NotificationManager.Show(new NotificationMessage("Physics colliders on"));
            Plugin.Log.LogInfo("Physics colliders turned on");
            Colliders.Reveal();
        }
    }
}
