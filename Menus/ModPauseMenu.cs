using MenuLib.API;

namespace SpeedrunMod.Menus;

internal static class ModPauseMenu
{
    public static void CreateMenu(PauseMenu menu)
    {
        ChapterSelectorMenu.CreateMenu(menu);
    }
}
