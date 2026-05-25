using SpeedrunMod.Events;
using SpeedrunMod.Practice.Chapters;
using SpeedrunMod.Practice.Minigames;
using SpeedrunMod.Utils;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice;

public static class PracticeManager
{
    private static bool _initialized;

    public static GameChapter CurrentChapter { get; set; } = ChapterResolver.None;

    public static ChapterMinigame CurrentMinigame { get; set; } = MinigameResolver.None;

    internal static void Initialize()
    {
        SceneLoadedEvent.SceneLoaded += OnSceneLoad;
        _initialized = true;
    }

    internal static void OnChapterLoad(GameChapter chapter, ChapterMinigame minigame)
    {
        CurrentChapter = chapter;
        CurrentMinigame = minigame;

        Plugin.Log.LogInfo($"PracticeManager.OnChapterLoad: chapter={chapter.Id}, minigame={minigame}");
    }

    internal static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        var oldChapter = CurrentChapter;
        var oldMinigame = CurrentMinigame;
        var chapter = ChapterResolver.ResolveChapter(scene.name);
        var minigame = ChapterResolver.ResolveMinigame(chapter, oldMinigame);

        if (chapter == null)
        {
            return;
        }

        Plugin.Log.LogInfo($"PracticeManager.OnSceneLoad: scene={scene.name}, mode={mode}, oldChapter={oldChapter.Id}, oldMinigame={oldMinigame}, chapter={chapter.Id}, minigame={minigame}");

        switch (chapter.Key)
        {
            case ChapterKey.MainMenu:
                CurrentChapter = ChapterResolver.None;
                CurrentMinigame = MinigameResolver.None;
                break;

            case ChapterKey.StartOfTheGame when minigame != null && minigame.Key == MinigameKey.TamagotchiCutting:
                TamagotchiCuttingMinigame.QueueLoad();
                break;

            case ChapterKey.StartOfTheGame when minigame != null && minigame.Key == MinigameKey.TamagotchiFull:
                TamagotchiFullMinigame.QueueLoad();
                break;

            case ChapterKey.InsideTheGame when oldMinigame != null && oldMinigame.Key == MinigameKey.TamagotchiFull:
                // Looping back to the start of the minigame for practice
                ChapterSelector.Load(ChapterKey.StartOfTheGame, MinigameKey.TamagotchiFull);
                return;

            case ChapterKey.ChibiMita when minigame != null && minigame.Key == MinigameKey.MakeMannequin:
                MannequinMinigame.QueueLoad();
                break;

            case ChapterKey.DummiesAndForgottenPuzzles when minigame != null && minigame.Key == MinigameKey.ConnectTheDots:
                ConnectTheDotsMinigame.QueueLoad();
                break;

            case ChapterKey.ReadingBooks when minigame != null && minigame.Key == MinigameKey.MilaMinigames:
                MilaMinigames.QueueLoad();
                break;
        }

        if (!chapter.IsPlayable)
        {
            return;
        }
        
        CurrentChapter = chapter;
        CurrentMinigame = minigame;
    }

    internal static void Update()
    {
        if (!_initialized) Initialize();

        if (CurrentMinigame == null || CurrentMinigame.Key == MinigameKey.None) return;

        Plugin.Log.LogInfo($"Update: CurrentMinigame={CurrentMinigame}", "PracticeManager", 5f);

        switch (CurrentMinigame.Key)
        {
            case MinigameKey.TamagotchiCutting:
                TamagotchiCuttingMinigame.Update();
                break;
            case MinigameKey.TamagotchiFull:
                TamagotchiFullMinigame.Update();
                break;
            case MinigameKey.MakeMannequin:
                MannequinMinigame.Update();
                break;
            case MinigameKey.ConnectTheDots:
                ConnectTheDotsMinigame.Update();
                break;
            case MinigameKey.MilaMinigames:
                MilaMinigames.Update();
                break;
        }
    }
}
