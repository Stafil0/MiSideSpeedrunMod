using System.Collections.Generic;
using SpeedrunMod.Practice.Minigames;

namespace SpeedrunMod.Practice.Chapters;

public sealed class GameChapter
{
    public ChapterKey Key { get; }
    public string Id { get; }
    public string DisplayName { get; }
    public string SceneName { get; }
    public string NameSave { get; }
    public int LevelLoad { get; }
    public int Order { get; }
    public bool IsPlayable { get; }
    public bool IsFastReloadable { get; }

    private readonly HashSet<MinigameKey> _supportedMinigames;

    internal GameChapter(
        ChapterKey key,
        string id,
        string displayName,
        string sceneName,
        string nameSave,
        int levelLoad,
        int order,
        bool isPlayable,
        bool isFastReloadable,
        IEnumerable<MinigameKey> supportedMinigames)
    {
        Key = key;
        Id = id;
        DisplayName = displayName;
        SceneName = sceneName;
        NameSave = nameSave;
        LevelLoad = levelLoad;
        Order = order;
        IsPlayable = isPlayable;
        IsFastReloadable = isFastReloadable;
        _supportedMinigames = new HashSet<MinigameKey>(supportedMinigames);
    }

    public bool SupportsMinigame(ChapterMinigame minigame)
    {
        if (minigame == null)
        {
            return true;
        }

        if (minigame.Key == MinigameKey.None)
        {
            return true;
        }

        return _supportedMinigames.Contains(minigame.Key);
    }

    public bool CanFastReload(ChapterMinigame minigame)
    {
        if (minigame == null || minigame.Key == MinigameKey.None || !SupportsMinigame(minigame))
        {
            return IsFastReloadable;
        }

        return minigame.IsFastReloadable;
    }

    public override string ToString() => DisplayName;

    public override bool Equals(object obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => Key.GetHashCode();

    public static bool operator ==(GameChapter left, GameChapter right) => ReferenceEquals(left, right);

    public static bool operator !=(GameChapter left, GameChapter right) => !ReferenceEquals(left, right);
}
