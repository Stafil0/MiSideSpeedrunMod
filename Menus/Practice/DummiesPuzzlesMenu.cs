using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.Minigames;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Menus.Practice;

public class DummiesPuzzlesMenu
{
    public static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("DUMMIES AND FORGOTTEN PUZZLES")
            .SetBackButton(previousMenu)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CONNECT THE DOTS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(LoadConnectTheDots)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CONNECT THE DOTS GAME 1")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(LoadConnectTheDotsGameOne)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CONNECT THE DOTS GAME 2")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(LoadConnectTheDotsGameTwo)
            .Build();

        return menu;
    }
    
    private static void LoadConnectTheDots()
    {
        // This value is set to 2 because during the first game load this value be switched to 2
        ConnectTheDotsMinigame.PlayingGame = 2;
        ConnectTheDotsMinigame.SwitchGames = true;
        ChapterSelector.Load(GameChapter.Ghostly, ChapterMinigame.ConnectTheDots);
    }
    
    private static void LoadConnectTheDotsGameOne()
    {
        ConnectTheDotsMinigame.PlayingGame = 1;
        ConnectTheDotsMinigame.SwitchGames = false;
        ChapterSelector.Load(GameChapter.Ghostly, ChapterMinigame.ConnectTheDots);
    }
    
    private static void LoadConnectTheDotsGameTwo()
    {
        ConnectTheDotsMinigame.PlayingGame = 2;
        ConnectTheDotsMinigame.SwitchGames = false;
        ChapterSelector.Load(GameChapter.Ghostly, ChapterMinigame.ConnectTheDots);
    }
}