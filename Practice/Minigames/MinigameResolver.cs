using System.Collections.Generic;

namespace SpeedrunMod.Practice.Minigames;

internal static class MinigameResolver
{
    private static readonly Dictionary<MinigameKey, ChapterMinigame> ByKey = new();

    internal static ChapterMinigame Get(MinigameKey key) => ByKey.TryGetValue(key, out var minigame)
        ? minigame
        : ByKey.GetValueOrDefault(MinigameKey.None);

    internal static ChapterMinigame None => Get(MinigameKey.None);

    static MinigameResolver()
    {
        Register(MinigameKey.None, sceneName: string.Empty, isFastReloadable: false);
        Register(MinigameKey.TamagotchiCutting, "Scene 1 - RealRoom", isFastReloadable: true);
        Register(MinigameKey.TamagotchiFull, "Scene 1 - RealRoom", isFastReloadable: true);
        Register(MinigameKey.MakeMannequin, "Scene 10 - ManekenWorld", isFastReloadable: false);
        Register(MinigameKey.ConnectTheDots, "Scene 11 - Backrooms", isFastReloadable: true);
        Register(MinigameKey.MilaMinigames, "Scene 19 - Glasses", isFastReloadable: true);
    }

    private static void Register(MinigameKey key, string sceneName, bool isFastReloadable = false)
    {
        ByKey[key] = new ChapterMinigame(key, sceneName, isFastReloadable);
    }
}
