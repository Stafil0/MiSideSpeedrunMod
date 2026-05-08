using SpeedrunMod.Events;
using SpeedrunMod.Practice.Minigames;
using SpeedrunMod.Utils;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice;

public static class PracticeManager
{
    private static bool _initialized;
    
    public static GameChapter CurrentChapter { get; set; } = GameChapter.None;

    public static ChapterMinigame CurrentMinigame { get; set; } = ChapterMinigame.None;

    internal static void Initialize()
    {
        SceneLoadedEvent.SceneLoaded += OnSceneLoad;
        _initialized = true;
    }

    internal static void OnChapterLoad(GameChapter chapter, ChapterMinigame minigame)
    {
        CurrentChapter = chapter;
        CurrentMinigame = minigame;
        
        Plugin.Log.LogInfo($"PracticeManager.OnChapterLoad: chapter={chapter}, minigame={minigame}");
    }
    
    internal static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        var oldChapter = CurrentChapter;
        var oldMinigame = CurrentMinigame;
        var chapter = ChapterResolver.ResolveChapter(scene.name);
        var minigame = ChapterResolver.ResolveMinigame(chapter, oldMinigame);

        Plugin.Log.LogInfo($"PracticeManager.OnSceneLoad: scene={scene.name}, mode={mode}, oldChapter={oldChapter}, oldMinigame={oldMinigame}, chapter={chapter}, minigame={minigame}");

        switch (chapter)
        {
            case GameChapter.MainMenu:
                CurrentChapter = GameChapter.None;
                CurrentMinigame = ChapterMinigame.None;
                break;
            case GameChapter.StartOfGame when minigame == ChapterMinigame.TamagotchiCutting:
                TamagotchiCuttingMinigame.QueueLoad();
                break;
            case GameChapter.StartOfGame when minigame == ChapterMinigame.TamagotchiFull:
                TamagotchiFullMinigame.QueueLoad();
                break;
            case GameChapter.InsideTheGame when oldMinigame == ChapterMinigame.TamagotchiFull:
                // looping back to the start of game to continue the tamagotchi full practice run
                ChapterSelector.Load(GameChapter.StartOfGame, ChapterMinigame.TamagotchiFull);
                return;
            case GameChapter.ChibiMita when minigame == ChapterMinigame.MakeMannequin:
                MannequinMinigame.QueueLoad();
                break;
            case GameChapter.DummiesAndForgottenPuzzles when minigame == ChapterMinigame.ConnectTheDots:
                ConnectTheDotsMinigame.QueueLoad();
                break;
            case GameChapter.ReadingBooks when minigame == ChapterMinigame.MilaMinigames:
                MilaMinigames.QueueLoad();
                break;
        }

        if (!chapter.IsPlayable()) return;

        CurrentChapter = chapter;
        CurrentMinigame = minigame;
    }

    internal static void Update()
    {
        if (!_initialized) Initialize();

        if (!CurrentMinigame.IsPlayable()) return;

        Plugin.Log.LogInfo($"Update: CurrentMinigame={CurrentMinigame}", "PracticeManager", 5f);

        switch (CurrentMinigame)
        {
            case ChapterMinigame.TamagotchiCutting:
                TamagotchiCuttingMinigame.Update();
                break;
            case ChapterMinigame.TamagotchiFull:
                TamagotchiFullMinigame.Update();
                break;
            case ChapterMinigame.MakeMannequin:
                MannequinMinigame.Update();
                break;
            case ChapterMinigame.ConnectTheDots:
                ConnectTheDotsMinigame.Update();
                break;
            case ChapterMinigame.MilaMinigames:
                MilaMinigames.Update();
                break;
            case ChapterMinigame.None:
                break;
        }
    }
}
