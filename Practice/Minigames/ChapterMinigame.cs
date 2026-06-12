namespace SpeedrunMod.Practice.Minigames;

public sealed class ChapterMinigame
{
    public MinigameKey Key { get; }
    public string SceneName { get; }
    public bool IsFastReloadable { get; }

    internal ChapterMinigame(MinigameKey key, string sceneName, bool isFastReloadable)
    {
        Key = key;
        SceneName = sceneName ?? string.Empty;
        IsFastReloadable = isFastReloadable;
    }

    public override string ToString() => Key.ToString();

    public override bool Equals(object obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => Key.GetHashCode();

    public static bool operator ==(ChapterMinigame left, ChapterMinigame right) => ReferenceEquals(left, right);

    public static bool operator !=(ChapterMinigame left, ChapterMinigame right) => !ReferenceEquals(left, right);
}
