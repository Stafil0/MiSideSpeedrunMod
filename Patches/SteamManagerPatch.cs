using HarmonyLib;
using SpeedrunMod.Notifications;
using SpeedrunMod.Menus.Frames;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Menus.FastReset;
using SpeedrunMod.Menus.Overlay;
using SpeedrunMod.Practice;
using SpeedrunMod.RevealSystems.Colliders;
using SpeedrunMod.RevealSystems.Interactables;
using SpeedrunMod.RevealSystems.Triggers;
using SpeedrunMod.Toggles;
using SpeedrunMod.Overlay;
using SpeedrunMod.Utils;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(SteamManager))]
internal class SteamManagerPatch
{
    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static void UpdatePatch()
    {
        NotificationManager.Update();
        VersionText.Update();
        OverlayManager.Update();
        KeybindCapture.Update();
        FastResetToggle.Update();
        OverlayToggle.Update();
        FpsOverrideToggle.Update();
        FpsUncapToggle.Update();
        EnableRunToggle.Update();
        RevealTriggerToggle.Update();
        RevealColliderToggle.Update();
        RevealInteractableToggle.Update();
        PracticeManager.Update();
        FpsSettingsMenu.Update();
        OverlaySettingsMenu.Update();
        FastResetSettingsMenu.Update();
        Triggers.Update();
        Colliders.Update();
        Interactables.Update();
    }
}