using MenuLib.API.Events;
using MenuLib.API;
using SpeedrunMod.Menus;

namespace SpeedrunMod.Events;

internal static class PauseMenuInitializedEvent
{
    internal static void RegisterEvent()
    {
        InitializedPauseMenuManagerEvent.Initialized += OnPauseMenuInitialized;
    }

    private static void OnPauseMenuInitialized(PauseMenu menu)
    {
        ModPauseMenu.CreateMenu(menu);
    }
}