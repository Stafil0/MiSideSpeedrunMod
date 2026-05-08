using System;

namespace SpeedrunMod.Practice;

internal static class ChapterResolver
{
    internal static string ResolveScene(GameChapter chapter, ChapterMinigame minigame)
    {
        if (chapter != GameChapter.None)
        {
            return chapter switch
            {
                // TODO: figure out scenes for commented out chapters
                GameChapter.Loading => "SceneLoading",
                GameChapter.MainMenu => "SceneMenu",
                GameChapter.StartOfTheGame => "Scene 1 - RealRoom",
                GameChapter.InsideTheGame => "Scene 2 - InGame",
                GameChapter.TogetherAtLast => "Scene 3 - WeTogether",
                GameChapter.ThingsGetWeird => "Scene 4 - StartSecret",
                GameChapter.ThingsGetScary => "Scene 5 - StartHorror",
                GameChapter.TheBasement => "Scene 6 - BasementFirst",
                GameChapter.BeyondTheWorld => "Scene 7 - Backrooms",
                // GameChapter.Cappie => "Scene 7 - Backrooms",
                GameChapter.TheLoop => "Scene 8 - ReRooms",
                GameChapter.ChibiMita => "Scene 9 - ChibiMita",
                GameChapter.ManekenWorld => "Scene 10 - ManekenWorld",
                GameChapter.DummiesAndForgottenPuzzles => "Scene 11 - Backrooms",
                // GameChapter.GhostMita => "Scene 11 - Backrooms",
                GameChapter.SheJustWantsToSleep => "Scene 17 - Dreamer",
                GameChapter.Novels => "Scene 18 - 2D",
                GameChapter.ReadingBooks => "Scene 19 - Glasses",
                GameChapter.RunAndHide => "Scene 20 - FightMita",
                GameChapter.OldVersion => "Scene 12 - Freak",
                GameChapter.BeingCandid => "Scene 14 - MobilePlayer",
                // GameChapter.TheRealWorld => "Scene 14 - MobilePlayer",
                GameChapter.Reboot => "Scene 15 - BasementAndDeath",
                // GameChapter.LeaveTheCore => "Scene 15 - BasementAndDeath",
                GameChapter.MainEnding => "Scene 16 - TheEnd",
                // GameChapter.StayEnding => "Scene 16 - TheEnd",
                // GameChapter.SafeEnding => "Scene 16 - TheEnd",
                _ => null,
            };
        }

        if (minigame != ChapterMinigame.None)
        {
            return minigame switch
            {
                ChapterMinigame.TamagotchiCutting => "Scene 1 - RealRoom",
                ChapterMinigame.TamagotchiFull => "Scene 1 - RealRoom",
                ChapterMinigame.MakeMannequin => "Scene 10 - ManekenWorld",
                ChapterMinigame.ConnectTheDots => "Scene 11 - Backrooms",
                ChapterMinigame.MilaMinigames => "Scene 19 - Glasses",
                _ => null,
            };
        }

        return null;
    }

    internal static GameChapter ResolveChapter(string scene)
    {
        return scene switch
        {
            "SceneLoading" => GameChapter.Loading,
            "SceneMenu" => GameChapter.MainMenu,
            "Scene 1 - RealRoom" => GameChapter.StartOfTheGame,
            "Scene 2 - InGame" => GameChapter.InsideTheGame,
            "Scene 3 - WeTogether" => GameChapter.TogetherAtLast,
            "Scene 4 - StartSecret" => GameChapter.ThingsGetWeird,
            "Scene 5 - StartHorror" => GameChapter.ThingsGetScary,
            "Scene 6 - BasementFirst" => GameChapter.TheBasement,
            "Scene 7 - Backrooms" => GameChapter.BeyondTheWorld,
            "Scene 8 - ReRooms" => GameChapter.TheLoop,
            "Scene 9 - ChibiMita" => GameChapter.ChibiMita,
            "Scene 10 - ManekenWorld" => GameChapter.ManekenWorld,
            "Scene 11 - Backrooms" => GameChapter.DummiesAndForgottenPuzzles,
            "Scene 17 - Dreamer" => GameChapter.SheJustWantsToSleep,
            "Scene 18 - 2D" => GameChapter.Novels,
            "Scene 19 - Glasses" => GameChapter.ReadingBooks,
            "Scene 20 - FightMita" => GameChapter.RunAndHide,
            "Scene 12 - Freak" => GameChapter.OldVersion,
            "Scene 14 - MobilePlayer" => GameChapter.BeingCandid,
            "Scene 15 - BasementAndDeath" => GameChapter.Reboot,
            "Scene 16 - TheEnd" => GameChapter.MainEnding,
            _ => GameChapter.None,
        };
    }

    internal static ChapterMinigame ResolveMinigame(GameChapter chapter, ChapterMinigame minigame)
    {
        if (!minigame.IsPlayable() || !chapter.IsPlayable())
        {
            return ChapterMinigame.None;
        }

        var resolvedMinigame = ChapterMinigame.None;
        if (chapter == GameChapter.StartOfTheGame)
        {
            switch (minigame)
            {
                case ChapterMinigame.TamagotchiCutting:
                case ChapterMinigame.TamagotchiFull:
                    resolvedMinigame = minigame;
                    break;
                default:
                    resolvedMinigame = ChapterMinigame.None;
                    break;
            }
        }

        if (chapter == GameChapter.ChibiMita)
        {
            switch (minigame)
            {
                case ChapterMinigame.MakeMannequin:
                    resolvedMinigame = minigame;
                    break;
                default:
                    resolvedMinigame = ChapterMinigame.None;
                    break;
            }
        }

        if (chapter == GameChapter.DummiesAndForgottenPuzzles)
        {
            switch (minigame)
            {
                case ChapterMinigame.ConnectTheDots:
                    resolvedMinigame = minigame;
                    break;
                default:
                    resolvedMinigame = ChapterMinigame.None;
                    break;
            }
        }

        if (chapter == GameChapter.ReadingBooks)
        {
            switch (minigame)
            {
                case ChapterMinigame.MilaMinigames:
                    resolvedMinigame = minigame;
                    break;
                default:
                    resolvedMinigame = ChapterMinigame.None;
                    break;
            }
        }

        return resolvedMinigame;
    }

    internal static GameChapter ResolvePreviousChapter(GameChapter chapter)
    {
        var current = (int)chapter;
        var previous = GameChapter.MainMenu;
        var previousValue = (int)previous;

        foreach (var c in Enum.GetValues<GameChapter>())
        {
            var v = (int)c;
            if (v >= current) continue;
            if (v > previousValue && c.IsPlayable())
            {
                previousValue = v;
                previous = c;
            }
        }

        return previous;
    }

    internal static GameChapter ResolveNextChapter(GameChapter chapter)
    {
        var current = (int)chapter;
        var next = GameChapter.None;
        var nextValue = int.MaxValue;

        foreach (var c in Enum.GetValues<GameChapter>())
        {
            var v = (int)c;
            if (v <= current) continue;
            if (v < nextValue && c.IsPlayable())
            {
                nextValue = v;
                next = c;
            }
        }

        return next;
    }
}
