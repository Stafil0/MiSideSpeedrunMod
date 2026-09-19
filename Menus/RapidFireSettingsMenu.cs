using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;

namespace SpeedrunMod.Menus;

internal static class RapidFireSettingsMenu
{
    private static MenuOption _enabledOption;

    private static string EnabledMenuLabel =>
        RapidFireInputsConfig.IsEnabled()
            ? "Rapid Fire: On"
            : "Rapid Fire: Off";

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("RAPID FIRE")
            .SetBackButton(previousMenu)
            .Build();

        _enabledOption = new MenuOptionFactory()
            .SetName(EnabledMenuLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(ToggleEnabled)
            .Build();

        return menu;
    }

    private static void ToggleEnabled()
    {
        RapidFireInputsConfig.Enabled.Value = !RapidFireInputsConfig.IsEnabled();
        RefreshEnabledText();
        Plugin.Log.LogInfo(EnabledMenuLabel);
    }

    private static void RefreshEnabledText()
    {
        SetMenuOptionText(_enabledOption, EnabledMenuLabel);
    }

    private static void SetMenuOptionText(MenuOption menuOption, string text)
    {
        if (menuOption == null) return;

        menuOption.Text = text;

        if (menuOption.TextComponent != null)
        {
            menuOption.TextComponent.text = text;
        }
    }
}
