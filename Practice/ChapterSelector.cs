using UnityEngine.SceneManagement;
using SpeedrunMod.Events;
using SpeedrunMod.Utils;

namespace SpeedrunMod.Practice;

public static class ChapterSelector
{
    private static GameChapter _queuedChapter = GameChapter.None;
    
    private static ChapterMinigame _queuedMinigame = ChapterMinigame.None;
    
    private static string _queuedScene = string.Empty;
    
    internal static bool IsQueued => !string.IsNullOrEmpty(_queuedScene);
    
    internal static GameChapter CurrentChapter { get; set; } = GameChapter.None;
    
    internal static ChapterMinigame CurrentMinigame { get; set; } = ChapterMinigame.None;
    

    internal static void Initialize()
    {
        Reset(force: true);
        SceneLoadedEvent.SceneLoaded += OnSceneLoad;
    }

    private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (!IsQueued)
        {
            CurrentChapter = ChapterResolver.ResolveChapter(scene.name);
            CurrentMinigame = ChapterResolver.ResolveMinigame(CurrentChapter, CurrentMinigame);
            
            Plugin.Log.LogInfo($"ChapterSelector.OnSceneLoaded: scene={scene.name}, mode={mode}, is queued={IsQueued}, current chapter={CurrentChapter}");
            
            return;
        }
        
        var loadedScene = ChapterResolver.ResolveChapter(scene.name);
        if (IsQueued && loadedScene == GameChapter.MainMenu)
        {
            Plugin.Log.LogInfo($"ChapterSelector.OnSceneLoaded: scene={scene.name}, mode={mode}, is queued={IsQueued}, current chapter={CurrentChapter}, loading queued scene={_queuedScene}, queued chapter={_queuedChapter}, queued minigame={_queuedMinigame}");
            
            Load(_queuedScene, _queuedChapter, _queuedMinigame);
            _queuedScene = string.Empty;
            _queuedChapter = GameChapter.None;
            _queuedMinigame = ChapterMinigame.None;
        }
    }

    internal static void NewGame()
    {
        Load(GameChapter.StartOfGame);
    }

    internal static void RestartChapter()
    {
        Load(CurrentChapter, CurrentMinigame);
    }

    internal static void Reset(bool force = false)
    {
        if (IsQueued && !force)
        {
            return;
        }

        CurrentChapter = GameChapter.None;
        CurrentMinigame = ChapterMinigame.None;
        _queuedScene = string.Empty;
        _queuedChapter = GameChapter.None;
        _queuedMinigame = ChapterMinigame.None;
    }

    internal static void Load(string scene)
    {
        var chapter = ChapterResolver.ResolveChapter(scene);
        if (!chapter.IsPlayable())
        {
            Plugin.Log.LogWarning($"ChapterSelector.Load: unsupported scene={scene}");
            return;
        }

        QueueLoad(scene, chapter, ChapterMinigame.None);
    }

    internal static void Load(GameChapter chapter, ChapterMinigame minigame = ChapterMinigame.None)
    {
        var scene = ChapterResolver.ResolveScene(chapter, minigame);
        if (string.IsNullOrEmpty(scene))
        {
            Plugin.Log.LogWarning($"ChapterSelector.Start: unsupported pair chapter={chapter}, minigame={minigame}");
            return;
        }

        var resolvedMinigame = ChapterResolver.ResolveMinigame(chapter, minigame);

        QueueLoad(scene, chapter, resolvedMinigame);
    }

    private static void QueueLoad(string scene, GameChapter chapter, ChapterMinigame minigame)
    {
        var isFastReloadable = IsFastReloadable(chapter, minigame);

        Plugin.Log.LogInfo($"ChapterSelector.QueueLoad: is fast reloadable={isFastReloadable}, scene={scene}, chapter={chapter}, minigame={minigame}");

        if (isFastReloadable || !GameUtil.IsInGame())
        {
            Load(scene, chapter, minigame);
            return;
        }

        _queuedScene = scene;
        _queuedChapter = chapter;
        _queuedMinigame = minigame;
        GameUtil.GetGameController().ExitGame();
    }

    private static void Load(string scene, GameChapter chapter, ChapterMinigame minigame)
    {
        Plugin.Log.LogInfo($"ChapterSelector.Load: loading scene={scene}, chapter={chapter}, minigame={minigame}");
        
        CurrentChapter = chapter;
        CurrentMinigame = minigame;
        GlobalGame.LoadingLevel = scene;
        SceneManager.LoadScene("SceneLoading");
        PracticeManager.OnChapterLoad(chapter, minigame);
    }

    private static bool IsFastReloadable(GameChapter chapter, ChapterMinigame minigame)
    {
        return (chapter, minigame) switch
        {
            (GameChapter.StartOfGame, ChapterMinigame.TamagotchiFull) => true,
            (GameChapter.StartOfGame, ChapterMinigame.TamagotchiCutting) => true,
            (GameChapter.DummiesAndForgottenPuzzles, ChapterMinigame.ConnectTheDots) => true,
            (GameChapter.ReadingBooks, _) => true,
#if DEBUG
            (GameChapter.StartOfGame, _) => false,
            (GameChapter.InsideTheGame, _) => false,
            (GameChapter.TogetherAtLast, _) => false,
            _ => true,
#else
            _ => false,
#endif
        };
    }
}
