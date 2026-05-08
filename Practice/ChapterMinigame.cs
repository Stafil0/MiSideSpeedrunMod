namespace SpeedrunMod.Practice;

public enum ChapterMinigame
{
    None,
    TamagotchiCutting,
    TamagotchiFull,
    MakeMannequin,
    ConnectTheDots,
    MilaMinigames,
}

public static class ChapterMinigameExtensions
{
    public static bool IsPlayable(this ChapterMinigame minigame)
    {
        return minigame switch
        {
            ChapterMinigame.None => false,
            _ => true,
        };
    }
}