using UnityEngine.SceneManagement;
using SpeedrunMod.Events;
using SpeedrunMod.Practice.Minigames;
using SpeedrunMod.Utils;

namespace SpeedrunMod.Practice.Chapters;

public static class ChapterSelector
{
    private static ChapterKey _queuedChapterId = ChapterKey.None;
    private static MinigameKey _queuedMinigameId = MinigameKey.None;

    internal static bool IsQueued => _queuedChapterId != ChapterKey.None;

    internal static GameChapter CurrentChapter { get; set; } = ChapterResolver.None;

    internal static ChapterMinigame CurrentMinigame { get; set; } = MinigameResolver.None;

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
        if (loadedScene != null && loadedScene.Key == ChapterKey.MainMenu)
        {
            Plugin.Log.LogInfo($"ChapterSelector.OnSceneLoaded: scene={scene.name}, mode={mode}, queued chapter={_queuedChapterId}, queued minigame={_queuedMinigameId}");

            Load(_queuedChapterId, _queuedMinigameId);
            
            _queuedChapterId = ChapterKey.None;
            _queuedMinigameId = MinigameKey.None;
        }
    }

    internal static void NewGame()
    {
        Load(ChapterKey.StartOfTheGame);
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

        CurrentChapter = ChapterResolver.Get(ChapterKey.None);
        CurrentMinigame = MinigameResolver.Get(MinigameKey.None);
        
        _queuedChapterId = ChapterKey.None;
        _queuedMinigameId = MinigameKey.None;
    }

    internal static void Load(string scene, bool fullReload = false)
    {
        var chapter = ChapterResolver.ResolveChapter(scene);
        if (!chapter.IsPlayable)
        {
            Plugin.Log.LogWarning($"ChapterSelector.Load: unsupported scene={scene}");
            return;
        }

        Load(chapter, MinigameResolver.None, fullReload);
    }

    internal static void Load(ChapterKey chapterId, MinigameKey minigameId = MinigameKey.None, bool fullReload = false)
    {
        var chapter = ChapterResolver.Get(chapterId);
        var minigame = MinigameResolver.Get(minigameId);

        if (chapter == null || minigame == null)
        {
            Plugin.Log.LogWarning($"ChapterSelector.Load: unsupported chapter={chapterId}, minigame={minigameId}");
            return;
        }

        Load(chapter, minigame, fullReload);
    }

    internal static void Load(GameChapter chapter, bool fullReload = false)
    {
        var minigame = MinigameResolver.None;

        if (chapter == null || minigame == null)
        {
            Plugin.Log.LogWarning($"ChapterSelector.Load: unsupported chapter={chapter?.Id}");
            return;
        }

        Load(chapter, minigame, fullReload);
    }

    internal static void Load(GameChapter chapter, ChapterMinigame minigame, bool fullReload = false)
    {
        if (chapter == null || minigame == null)
        {
            Plugin.Log.LogWarning($"ChapterSelector.Load: unsupported chapter={chapter?.Id}, minigame={minigame?.Key}");
            return;
        }

        var scene = ChapterResolver.ResolveScene(chapter, minigame);
        if (string.IsNullOrEmpty(scene))
        {
            Plugin.Log.LogWarning($"ChapterSelector.Load: unsupported pair chapter={chapter.Id}, minigame={minigame}");
            return;
        }

        var resolvedMinigame = ChapterResolver.ResolveMinigame(chapter, minigame);
        QueueLoad(chapter, resolvedMinigame, fullReload);
    }

    private static void QueueLoad(GameChapter chapter, ChapterMinigame minigame, bool fullReload)
    {
        var isFastReloadable = IsFastReloadable(chapter, minigame);

        Plugin.Log.LogInfo($"ChapterSelector.QueueLoad: is fast reloadable={isFastReloadable}, fullReload={fullReload}, scene={chapter.SceneName}, chapter={chapter.Id}, minigame={minigame}");

        if (!fullReload && (isFastReloadable || !GameUtil.IsInGame()))
        {
            DispatchLoad(chapter, minigame);
            return;
        }

        _queuedChapterId = chapter.Key;
        _queuedMinigameId = minigame.Key;
        GameUtil.GetGameController().ExitGame();
    }

    private static bool IsFastReloadable(GameChapter target, ChapterMinigame minigame)
    {
        if (CurrentChapter == null || target == null)
        {
            return false;
        }

        if (CurrentChapter.Key == ChapterKey.Novels && target.Key != ChapterKey.Novels)
        {
            return false;
        }

        return target.CanFastReload(minigame);
    }

    private static void DispatchLoad(GameChapter chapter, ChapterMinigame minigame)
    {
        Plugin.Log.LogInfo($"ChapterSelector.DispatchLoad: scene={chapter.SceneName}, chapter={chapter.Id}, minigame={minigame}");

        CurrentChapter = chapter;
        CurrentMinigame = minigame;
        PracticeManager.OnChapterLoad(chapter, minigame);

        GlobalGame.LoadingLevel = chapter.SceneName;
        GlobalGame.nameLoadedScene = chapter.SceneName;
        GlobalGame.levelLoad = chapter.LevelLoad;
        GlobalGame.playWorld = false;
        GlobalGame.timeGameplay = false;
        Location4ChangeSide.ResetData();

        SceneManager.LoadScene("SceneLoading");
        Plugin.Log.LogInfo($"ChapterSelector.DispatchLoad: levelLoad={chapter.LevelLoad} nameSave={chapter.NameSave}");
    }
}
