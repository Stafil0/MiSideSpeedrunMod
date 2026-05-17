using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice.Chapters;
using SpeedrunMod.Practice.Minigames;

namespace SpeedrunMod.Menus.Practice;

public static class StartOfGamePracticeMenu
{
    public static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("START OF GAME")
            .SetBackButton(previousMenu)
            .Build();
        
        new MenuOptionFactory()
            .SetName("FULL TAMAGOTCHI RUN")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(LoadTamagotchiFullRun)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CARROT CUTTING")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(Load2DCutting)
            .Build();

        return menu;
    }
    
    private static void LoadTamagotchiFullRun()
    {
        ChapterSelector.Load(ChapterKey.StartOfTheGame, MinigameKey.TamagotchiFull);
    }
    
    private static void Load2DCutting()
    {
        ChapterSelector.Load(ChapterKey.StartOfTheGame, MinigameKey.TamagotchiCutting);
    }
}