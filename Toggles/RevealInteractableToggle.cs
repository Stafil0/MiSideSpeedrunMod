using SpeedrunMod.Notifications;
using SpeedrunMod.RevealSystems.Interactables;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class RevealInteractableToggle
{
    internal static void Update()
    {
        if (!Input.GetKey(KeyCode.LeftAlt) || !Input.GetKeyDown(KeyCode.I))
        {
            return;
        }

        if (Interactables.IsRevealing())
        {
            NotificationManager.Show(new NotificationMessage("Interactable colliders off"));
            Plugin.Log.LogInfo("Interactable colliders turned off");
            Interactables.Hide();
        }
        else
        {
            NotificationManager.Show(new NotificationMessage("Interactable colliders on"));
            Plugin.Log.LogInfo("Interactable colliders turned on");
            Interactables.Reveal();
        }
    }
}
