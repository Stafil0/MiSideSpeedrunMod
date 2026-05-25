using SpeedrunMod.Notifications;
using SpeedrunMod.RevealSystems.Triggers;

namespace SpeedrunMod.Toggles;

internal static class RevealTriggerToggle
{
    internal static void Update()
    {
        if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.O))
        {
            return;
        }
        
        if (Triggers.IsRevealing())
        {
            NotificationManager.Show(new NotificationMessage("Trigger colliders turned off"));
            Plugin.Log.LogInfo("Trigger colliders turned off");
            Triggers.Hide();
        }
        else
        {
            NotificationManager.Show(new NotificationMessage("Trigger colliders turned on"));
            Plugin.Log.LogInfo("Trigger colliders turned on");
            Triggers.Reveal();
        }
    }
}