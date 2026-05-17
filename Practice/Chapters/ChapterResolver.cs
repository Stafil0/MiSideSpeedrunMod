using System;
using System.Collections.Generic;
using SpeedrunMod.Practice.Minigames;

namespace SpeedrunMod.Practice.Chapters;

internal static class ChapterResolver
{
    private static readonly List<GameChapter> All = new();
    private static readonly Dictionary<ChapterKey, GameChapter> ByKey = new();
    private static readonly Dictionary<(string, int), GameChapter> ByScene = new();
    private static readonly List<GameChapter> PlayableChapters = new();
    
    internal static IReadOnlyList<GameChapter> Playable => PlayableChapters;

    internal static GameChapter None => Get(ChapterKey.None);

    internal static GameChapter Get(ChapterKey key) => ByKey.TryGetValue(key, out var chapter)
        ? chapter
        : ByKey.GetValueOrDefault(ChapterKey.None);

    static ChapterResolver()
    {
#if DEBUG
        // TODO: Delete after testing all chapters for fast reloadability
        #region Debug Chapters

        Register(ChapterKey.None, "NONE", string.Empty, order: -1, isPlayable: false);
        Register(ChapterKey.Loading, "LOADING", "SceneLoading", order: -2, isPlayable: false);
        Register(ChapterKey.MainMenu, "MAIN MENU", "SceneMenu", order: 0, isPlayable: false);

        Register(
            ChapterKey.StartOfTheGame,
            "START OF THE GAME",
            "Scene 1 - RealRoom",
            order: 1,
            isFastReloadable: false,
            supportedMinigames: new[] { MinigameKey.TamagotchiCutting, MinigameKey.TamagotchiFull });

        Register(ChapterKey.InsideTheGame, "I'M INSIDE A GAME?", "Scene 2 - InGame", order: 2, isFastReloadable: false);
        Register(ChapterKey.TogetherAtLast, "TOGETHER AT LAST", "Scene 3 - WeTogether", order: 3, isFastReloadable: false);

        Register(ChapterKey.ThingsGetWeird, "THINGS GET WEIRD", "Scene 4 - StartSecret", order: 4, isFastReloadable: true);
        Register(ChapterKey.ThingsGetScary, "THINGS GET WEIRD (HORROR)", "Scene 5 - StartHorror", order: 5, isFastReloadable: true);
        Register(ChapterKey.TheBasement, "THE BASEMENT", "Scene 6 - BasementFirst", order: 6, isFastReloadable: true);
        Register(ChapterKey.BeyondTheWorld, "BEYOND THE WORLD", "Scene 7 - Backrooms", order: 7, isFastReloadable: true);
        Register(ChapterKey.Cappie, "CAPPIE", "Scene 7 - Backrooms", order: 8, levelLoad: 1, isFastReloadable: true);
        Register(ChapterKey.TheLoop, "THE LOOP", "Scene 8 - ReRooms", order: 9, isFastReloadable: true);

        Register(
            ChapterKey.ChibiMita,
            "CHIBI MITA",
            "Scene 9 - ChibiMita",
            order: 10,
            supportedMinigames: new[] { MinigameKey.MakeMannequin },
            isFastReloadable: true);

        Register(ChapterKey.ManekenWorld, "MANEKEN WORLD", "Scene 10 - ManekenWorld", order: 11, isFastReloadable: true);

        Register(
            ChapterKey.DummiesAndForgottenPuzzles,
            "DUMMIES AND FORGOTTEN PUZZLES",
            "Scene 11 - Backrooms",
            order: 12,
            supportedMinigames: new[] { MinigameKey.ConnectTheDots },
            isFastReloadable: true);

        Register(ChapterKey.GhostMita, "GHOST MITA", "Scene 11 - Backrooms", order: 13, levelLoad: 1, isFastReloadable: true);
        Register(ChapterKey.SheJustWantsToSleep, "SHE JUST WANTS TO SLEEP", "Scene 17 - Dreamer", order: 14, isFastReloadable: true);
        Register(ChapterKey.Novels, "NOVELS", "Scene 18 - 2D", order: 15, isFastReloadable: true);

        Register(
            ChapterKey.ReadingBooks,
            "READING BOOKS, DESTROYING GLITCHES",
            "Scene 19 - Glasses",
            order: 16,
            supportedMinigames: new[] { MinigameKey.MilaMinigames },
            isFastReloadable: true);

        Register(ChapterKey.RunAndHide, "RUN AND HIDE!", "Scene 20 - FightMita", order: 17, isFastReloadable: true);
        Register(ChapterKey.OldVersion, "OLD VERSION", "Scene 12 - Freak", order: 18, isFastReloadable: true);
        Register(ChapterKey.BeingCandid, "BEING CANDID", "Scene 14 - MobilePlayer", order: 19, isFastReloadable: true);
        Register(ChapterKey.TheRealWorld, "THE REAL WORLD", "Scene 14 - MobilePlayer", order: 20, levelLoad: 1, isFastReloadable: true);
        Register(ChapterKey.Reboot, "REBOOT", "Scene 15 - BasementAndDeath", order: 21, isFastReloadable: true);
        Register(ChapterKey.LeaveTheCore, "LEAVE THE CORE", "Scene 15 - BasementAndDeath", order: 22, levelLoad: 1, isFastReloadable: true);
        Register(ChapterKey.MainEnding, "THE END", "Scene 16 - TheEnd", order: 23);
        Register(ChapterKey.StayEnding, "THE END (STAY)", "Scene 16 - TheEnd", order: 24, levelLoad: 1);
        Register(ChapterKey.SafeEnding, "THE END (SAFE)", "Scene 16 - TheEnd", order: 25, levelLoad: 2);

        #endregion
#else
        #region Real Chapters

        Register(ChapterKey.None, "NONE", string.Empty, order: -1, isPlayable: false);
        Register(ChapterKey.Loading, "LOADING", "SceneLoading", order: -2, isPlayable: false);
        Register(ChapterKey.MainMenu, "MAIN MENU", "SceneMenu", order: 0, isPlayable: false);

        Register(
            ChapterKey.StartOfTheGame,
            "START OF THE GAME",
            "Scene 1 - RealRoom",
            order: 1,
            supportedMinigames: new[] { MinigameKey.TamagotchiCutting, MinigameKey.TamagotchiFull });

        Register(ChapterKey.InsideTheGame, "I'M INSIDE A GAME?", "Scene 2 - InGame", order: 2);
        Register(ChapterKey.TogetherAtLast, "TOGETHER AT LAST", "Scene 3 - WeTogether", order: 3);

        Register(ChapterKey.ThingsGetWeird, "THINGS GET WEIRD", "Scene 4 - StartSecret", order: 4);
        Register(ChapterKey.ThingsGetScary, "THINGS GET WEIRD (HORROR)", "Scene 5 - StartHorror", order: 5);
        Register(ChapterKey.TheBasement, "THE BASEMENT", "Scene 6 - BasementFirst", order: 6);
        Register(ChapterKey.BeyondTheWorld, "BEYOND THE WORLD", "Scene 7 - Backrooms", order: 7);
        Register(ChapterKey.Cappie, "CAPPIE", "Scene 7 - Backrooms", order: 8, levelLoad: 1);
        Register(ChapterKey.TheLoop, "THE LOOP", "Scene 8 - ReRooms", order: 9);

        Register(
            ChapterKey.ChibiMita,
            "CHIBI MITA",
            "Scene 9 - ChibiMita",
            order: 10,
            supportedMinigames: new[] { MinigameKey.MakeMannequin });

        Register(ChapterKey.ManekenWorld, "MANEKEN WORLD", "Scene 10 - ManekenWorld", order: 11);

        Register(
            ChapterKey.DummiesAndForgottenPuzzles,
            "DUMMIES AND FORGOTTEN PUZZLES",
            "Scene 11 - Backrooms",
            order: 12,
            supportedMinigames: new[] { MinigameKey.ConnectTheDots });

        Register(ChapterKey.GhostMita, "GHOST MITA", "Scene 11 - Backrooms", order: 13, levelLoad: 1);
        Register(ChapterKey.SheJustWantsToSleep, "SHE JUST WANTS TO SLEEP", "Scene 17 - Dreamer", order: 14);
        Register(ChapterKey.Novels, "NOVELS", "Scene 18 - 2D", order: 15);

        Register(
            ChapterKey.ReadingBooks,
            "READING BOOKS, DESTROYING GLITCHES",
            "Scene 19 - Glasses",
            order: 16,
            supportedMinigames: new[] { MinigameKey.MilaMinigames },
            isFastReloadable: true);

        Register(ChapterKey.RunAndHide, "RUN AND HIDE!", "Scene 20 - FightMita", order: 17);
        Register(ChapterKey.OldVersion, "OLD VERSION", "Scene 12 - Freak", order: 18);
        Register(ChapterKey.BeingCandid, "BEING CANDID", "Scene 14 - MobilePlayer", order: 19);
        Register(ChapterKey.TheRealWorld, "THE REAL WORLD", "Scene 14 - MobilePlayer", order: 20, levelLoad: 1);
        Register(ChapterKey.Reboot, "REBOOT", "Scene 15 - BasementAndDeath", order: 21);
        Register(ChapterKey.LeaveTheCore, "LEAVE THE CORE", "Scene 15 - BasementAndDeath", order: 22, levelLoad: 1);
        Register(ChapterKey.MainEnding, "THE END", "Scene 16 - TheEnd", order: 23);
        Register(ChapterKey.StayEnding, "THE END (STAY)", "Scene 16 - TheEnd", order: 24, levelLoad: 1, isPlayable: false);
        Register(ChapterKey.SafeEnding, "THE END (SAFE)", "Scene 16 - TheEnd", order: 25, levelLoad: 2, isPlayable: false);

        #endregion
#endif
    }

    internal static string ResolveScene(GameChapter chapter, ChapterMinigame minigame)
    {
        if (minigame != null && minigame.Key != MinigameKey.None && !string.IsNullOrEmpty(minigame.SceneName))
        {
            return minigame.SceneName;
        }

        return chapter != null ? chapter.SceneName : string.Empty;
    }

    internal static GameChapter ResolveChapter(string scene)
    {
        return ResolveChapter(scene, GlobalGame.levelLoad);
    }

    internal static GameChapter ResolveChapter(string scene, int levelLoad)
    {
        if (ByScene.TryGetValue((scene, levelLoad), out var chapter) || ByScene.TryGetValue((scene, 0), out chapter))
        {
            return chapter;
        }

        return Get(ChapterKey.None);
    }

    internal static ChapterMinigame ResolveMinigame(GameChapter chapter, ChapterMinigame minigame)
    {
        if (minigame == null || minigame.Key == MinigameKey.None || chapter == null || !chapter.IsPlayable)
        {
            return MinigameResolver.None;
        }

        return chapter.SupportsMinigame(minigame) ? minigame : MinigameResolver.None;
    }

    internal static GameChapter ResolvePreviousChapter(GameChapter chapter)
    {
        if (chapter == null)
        {
            return None;
        }

        GameChapter previous = None;
        var found = false;

        foreach (var candidate in All)
        {
            if (candidate.Order >= chapter.Order || !candidate.IsPlayable)
            {
                continue;
            }

            if (!found || candidate.Order > previous.Order)
            {
                previous = candidate;
                found = true;
            }
        }

        return previous;
    }

    internal static GameChapter ResolveNextChapter(GameChapter chapter)
    {
        if (chapter == null)
        {
            return None;
        }

        GameChapter next = None;
        var found = false;

        foreach (var candidate in All)
        {
            if (candidate.Order <= chapter.Order || !candidate.IsPlayable)
            {
                continue;
            }

            if (!found || candidate.Order < next.Order)
            {
                next = candidate;
                found = true;
            }
        }

        return next;
    }

    private static GameChapter Register(
        ChapterKey key,
        string displayName,
        string sceneName,
        int order,
        bool isPlayable = true,
        bool isFastReloadable = false,
        int levelLoad = 0,
        string nameSave = null,
        IEnumerable<MinigameKey> supportedMinigames = null)
    {
        var id = key.ToString();
        var chapter = new GameChapter(
            key,
            id,
            displayName,
            sceneName,
            nameSave: nameSave ?? id,
            levelLoad,
            order,
            isPlayable,
            isFastReloadable,
            supportedMinigames ?? Array.Empty<MinigameKey>());

        All.Add(chapter);
        ByKey[key] = chapter;

        if (!string.IsNullOrEmpty(sceneName))
        {
            ByScene.TryAdd((sceneName, levelLoad), chapter);
        }

        if (isPlayable)
        {
            PlayableChapters.Add(chapter);
        }

        return chapter;
    }
}
