using SpeedrunMod.Events;
using SpeedrunMod.Practice.Minigames;
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
    }
    
    internal static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        CurrentChapter = ChapterResolver.Resolve(scene.name);
        CurrentMinigame = ChapterResolver.ResolveMinigame(CurrentChapter, CurrentMinigame);

        if (!CurrentChapter.IsPlayable() && !CurrentMinigame.IsPlayable()) return;

        switch (CurrentChapter, CurrentMinigame)
        {
            case (GameChapter.MainMenu, _):
                CurrentChapter = GameChapter.None;
                CurrentMinigame = ChapterMinigame.None;
                break;
            case (GameChapter.StartOfGame, ChapterMinigame.TamagotchiCutting):
                TamagotchiCuttingMinigame.QueueLoad();
                break;
            case (GameChapter.StartOfGame, ChapterMinigame.TamagotchiFull):
                TamagotchiFullMinigame.QueueLoad();
                break;
            case (GameChapter.InsideTheGame, ChapterMinigame.TamagotchiFull):
                // looping back to the start of game to continue the tamagotchi full practice run
                ChapterSelector.Load(GameChapter.StartOfGame, ChapterMinigame.TamagotchiFull);
                break;
            case (GameChapter.ChibiMita, ChapterMinigame.MakeMannequin):
                MannequinMinigame.QueueLoad();
                break;
            case (GameChapter.Ghostly, ChapterMinigame.ConnectTheDots):
                ConnectTheDotsMinigame.QueueLoad();
                break;
            case (GameChapter.ReadingBooks, ChapterMinigame.MilaMinigames):
                MilaMinigames.QueueLoad();
                break;
            case (_, _):
                break;
        }
    }

    internal static void Update()
    {
        if (!_initialized) Initialize();

        if (!CurrentMinigame.IsPlayable()) return;

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
