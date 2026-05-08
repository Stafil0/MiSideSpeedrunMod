namespace SpeedrunMod.Practice;

// TODO: figure out scenes for commented out chapters
public enum GameChapter
{
    Loading = -2,
    None = -1,
    MainMenu = 0,
    StartOfTheGame = 1,
    InsideTheGame = 2,
    TogetherAtLast = 3,
    ThingsGetWeird = 4,
    ThingsGetScary = 5,
    TheBasement = 6,
    BeyondTheWorld = 7,
    // Cappie = 8,
    TheLoop = 9,
    ChibiMita = 10,
    ManekenWorld = 11,
    DummiesAndForgottenPuzzles = 12,
    // GhostMita = 13,
    SheJustWantsToSleep = 14,
    Novels = 15,
    ReadingBooks = 16,
    RunAndHide = 17,
    OldVersion = 18,
    BeingCandid = 19,
    // TheRealWorld = 20,
    Reboot = 21,
    // LeaveTheCore = 22,
    MainEnding = 23,
    // StayEnding = 24,
    // SafeEnding = 25,
}

public static class GameChapterExtensions
{
    public static string ToDisplayName(this GameChapter chapter)
    {
        return chapter switch
        {
            GameChapter.None => "NONE",
            GameChapter.Loading => "LOADING",
            GameChapter.MainMenu => "MAIN MENU",
            GameChapter.StartOfTheGame => "START OF THE GAME",
            GameChapter.InsideTheGame => "I'M INSIDE A GAME?",
            GameChapter.TogetherAtLast => "TOGETHER AT LAST",
            GameChapter.ThingsGetWeird => "THINGS GET WEIRD",
            GameChapter.ThingsGetScary => "THINGS GET WEIRD (HORROR)",
            GameChapter.TheBasement => "THE BASEMENT",
            GameChapter.BeyondTheWorld => "BEYOND THE WORLD",
            // GameChapter.Cappie => "CAPPIE",
            GameChapter.TheLoop => "THE LOOP",
            GameChapter.ChibiMita => "CHIBI MITA",
            GameChapter.ManekenWorld => "MANEKEN WORLD",
            GameChapter.DummiesAndForgottenPuzzles => "DUMMIES AND FORGOTTEN PUZZLES",
            // GameChapter.GhostMita => "GHOST MITA",
            GameChapter.SheJustWantsToSleep => "SHE JUST WANTS TO SLEEP",
            GameChapter.Novels => "NOVELS",
            GameChapter.ReadingBooks => "READING BOOKS, DESTROYING GLITCHES",
            GameChapter.RunAndHide => "RUN AND HIDE!",
            GameChapter.OldVersion => "OLD VERSION",
            GameChapter.BeingCandid => "BEING CANDID",
            // GameChapter.TheRealWorld => "THE REAL WORLD",
            GameChapter.Reboot => "REBOOT",
            // GameChapter.LeaveTheCore => "\"LEAVE THE CORE!\"",
            GameChapter.MainEnding => "THE END",
            // GameChapter.StayEnding => "STAY ENDING",
            // GameChapter.SafeEnding => "SAFE ENDING",
            _ => chapter.ToString(),
        };
    }

    public static bool IsPlayable(this GameChapter chapter)
    {
        return chapter switch
        {
            GameChapter.None => false,
            GameChapter.Loading => false,
            GameChapter.MainMenu => false,
            GameChapter.MainEnding => false,
            // GameChapter.StayEnding => false,
            // GameChapter.SafeEnding => false,
            _ => true,
        };
    }
}
