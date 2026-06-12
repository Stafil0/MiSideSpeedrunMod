using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;
using SpeedrunMod.Menus.Keybinds;
using UnityEngine;

namespace SpeedrunMod.Menus.FastReset;

internal static class FastResetSettingsMenu
{
    private const string CaptureContext = "FastResetSettingsMenu";

    private static MenuOption _enableOption;
    private static MenuOption _holdSecondsOption;
    private static MenuOption _resetChapterKeyOption;

    private static string EnableLabel =>
        FastResetConfig.EnableFastReset.Value ? "Fast reset: On" : "Fast reset: Off";

    private static string HoldSecondsLabel =>
        $"Hold duration: {FastResetConfig.HoldSeconds.Value:F2} s";

    private static string ResetChapterKeyLabel =>
        $"Reset chapter key: {FastResetConfig.ResetChapterKey.Value}";

    private static string RestartInfoLabel =>
        "Hold Reset Button: restart current chapter";

    private static string PreviousInfoLabel =>
        $"Hold Reset Button + {FastResetConfig.PreviousChapterKey.Value}: load previous chapter";

    private static string NextInfoLabel =>
        $"Hold Reset Button + {FastResetConfig.NextChapterKey.Value}: load next chapter";

    private static string NewGameInfoLabel =>
        $"Hold Reset Button + {FastResetConfig.NewGameKey.Value}: start new game";

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("FAST RESET")
            .SetBackButton(previousMenu)
            .Build();

        new MenuOptionFactory()
            .SetName(RestartInfoLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();

        new MenuOptionFactory()
            .SetName(PreviousInfoLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();

        new MenuOptionFactory()
            .SetName(NextInfoLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();

        new MenuOptionFactory()
            .SetName(NewGameInfoLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();

        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();

        _enableOption = new MenuOptionFactory()
            .SetName(EnableLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(ToggleEnable)
            .Build();

        _resetChapterKeyOption = new MenuOptionFactory()
            .SetName(ResetChapterKeyLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(BeginResetChapterKeyCapture)
            .Build();

        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();

        _holdSecondsOption = new MenuOptionFactory()
            .SetName(HoldSecondsLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();

        new MenuOptionFactory()
            .SetName("+1 s")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustHoldSeconds(1f))
            .Build();

        new MenuOptionFactory()
            .SetName("-1 s")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustHoldSeconds(-1f))
            .Build();

        return menu;
    }

    internal static void Update()
    {
        if (!KeybindCapture.IsCapturing(CaptureContext)) return;
        if (!IsMenuVisible() && KeybindCapture.CancelCapture(CaptureContext))
        {
            RefreshResetChapterKeyText();
            Plugin.Log.LogInfo("Fast reset settings key capture cancelled (left FAST RESET menu).");
        }
    }

    private static void ToggleEnable()
    {
        FastResetConfig.EnableFastReset.Value = !FastResetConfig.EnableFastReset.Value;
        RefreshEnableText();
        Plugin.Log.LogInfo(EnableLabel);
    }

    private static void AdjustHoldSeconds(float delta)
    {
        var v = Mathf.Clamp(FastResetConfig.HoldSeconds.Value + delta, 1f, 10f);
        FastResetConfig.HoldSeconds.Value = v;
        RefreshHoldSecondsText();
        Plugin.Log.LogInfo($"Fast reset hold duration set to {v:F2} s.");
    }

    private static void BeginResetChapterKeyCapture()
    {
        SetMenuOptionText(_resetChapterKeyOption, "Restart chapter key: <press key... Esc to cancel>");
        KeybindCapture.BeginCapture(CaptureContext, OnResetChapterKeyCaptureComplete);
    }

    private static void OnResetChapterKeyCaptureComplete(bool success, KeyCode keyCode)
    {
        if (success)
        {
            FastResetConfig.ResetChapterKey.Value = keyCode;
            RefreshResetChapterKeyText();
            Plugin.Log.LogInfo($"Fast reset restart-chapter key set to {keyCode}.");
            return;
        }

        RefreshResetChapterKeyText();
    }

    private static void RefreshEnableText() => SetMenuOptionText(_enableOption, EnableLabel);

    private static void RefreshHoldSecondsText() => SetMenuOptionText(_holdSecondsOption, HoldSecondsLabel);

    private static void RefreshResetChapterKeyText() => SetMenuOptionText(_resetChapterKeyOption, ResetChapterKeyLabel);

    private static void SetMenuOptionText(MenuOption menuOption, string text)
    {
        if (menuOption == null) return;

        menuOption.Text = text;

        if (menuOption.TextComponent != null)
        {
            menuOption.TextComponent.text = text;
        }
    }

    private static bool IsMenuVisible()
    {
        return _enableOption != null &&
               _enableOption.TextComponent != null &&
               _enableOption.TextComponent.gameObject.activeInHierarchy;
    }
}
