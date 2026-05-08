namespace SpeedrunMod.Practice;

// TODO: figure out scenes for commented out chapters
public enum GameChapter
{
    Loading = -2,
    None = -1,
    MainMenu = 0,
    StartOfGame = 1,
    InsideTheGame = 2,
    TogetherAtLast = 3,
    ThingsGetWeird = 4,
    TheBasement = 5,
    BeyondTheWorld = 6,
    // Cappie = 7,
    TheLoop = 8,
    ChibiMita = 9,
    // DummiesAndForgottenPuzzles = 10,
    Ghostly = 11,
    SheJustWantsToSleep = 12,
    // Novels = 13,
    ReadingBooks = 14,
    RunAndHide = 15,
    OldVersion = 16,
    // BeingCandid = 17,
    TheRealWorld = 18,
    // Reboot = 19,
    MainEnding = 20,
    StayEnding = 21,
    SafeEnding = 22,
}

public static class GameChapterExtensions
{
    public static bool IsPlayable(this GameChapter chapter)
    {
        return chapter switch
        {
            GameChapter.None => false,
            GameChapter.Loading => false,
            GameChapter.MainMenu => false,
            GameChapter.MainEnding => false,
            GameChapter.StayEnding => false,
            GameChapter.SafeEnding => false,
            _ => true,
        };
    }
}
