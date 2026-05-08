using SpeedrunMod.Configs;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Notifications;
using SpeedrunMod.Practice;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class FastResetToggle
{
    private enum FastResetAction
    {
        RestartChapter,
        NewGame,
        PreviousChapter,
        NextChapter,
    }

    private const float PreviewIntervalSeconds = 1f;
    private static float? _holdStartRealtime;
    private static float? _lastPreviewRealtime;

    internal static void Update()
    {
        if (!FastResetConfig.EnableFastReset.Value) return;
        if (KeybindCapture.IsCapturing()) return;
        if (!GameUtil.IsInGame()) return;

        var chapterKey = FastResetConfig.ResetChapterKey.Value;

        if (Input.GetKeyDown(chapterKey))
        {
            _holdStartRealtime = Time.realtimeSinceStartup;
            _lastPreviewRealtime = null;
        }

        if (!_holdStartRealtime.HasValue || !Input.GetKey(chapterKey))
        {
            _holdStartRealtime = null;
            _lastPreviewRealtime = null;
            return;
        }

        var elapsed = Time.realtimeSinceStartup - _holdStartRealtime.Value;
        var holdNeeded = FastResetConfig.HoldSeconds.Value;
        var action = ResolveAction();

        if (elapsed < holdNeeded)
        {
            var now = Time.realtimeSinceStartup;
            var intervalElapsed = !_lastPreviewRealtime.HasValue || now - _lastPreviewRealtime.Value >= PreviewIntervalSeconds;
            if (!intervalElapsed) return;

            _lastPreviewRealtime = now;
            var remaining = Mathf.Max(0f, holdNeeded - elapsed);
            var preview = BuildNotificationMessage(action, remaining);
            
            if (!string.IsNullOrEmpty(preview))
            {
                NotificationManager.Show(new NotificationMessage(preview));
            }

            return;
        }

        _holdStartRealtime = null;
        _lastPreviewRealtime = null;

        switch (action)
        {
            case FastResetAction.NewGame:
                ChapterSelector.NewGame();
                return;
            case FastResetAction.PreviousChapter:
            {
                var previousChapter = ChapterResolver.ResolvePreviousChapter(ChapterSelector.CurrentChapter);
                if (!previousChapter.IsPlayable())
                {
                    Plugin.Log.LogWarning($"FastResetToggle.Update: no previous chapter found for {ChapterSelector.CurrentChapter}");
                    return;
                }

                ChapterSelector.Load(previousChapter);
                return;
            }
            case FastResetAction.NextChapter:
            {
                var nextChapter = ChapterResolver.ResolveNextChapter(ChapterSelector.CurrentChapter);
                if (!nextChapter.IsPlayable())
                {
                    Plugin.Log.LogWarning($"FastResetToggle.Update: no next chapter found for {ChapterSelector.CurrentChapter}");
                    return;
                }

                ChapterSelector.Load(nextChapter);
                return;
            }
            case FastResetAction.RestartChapter:
            {
                if (!ChapterSelector.CurrentChapter.IsPlayable())
                {
                    Plugin.Log.LogWarning($"FastResetToggle.Update: current chapter '{ChapterSelector.CurrentChapter}' is not playable");
                    return;
                }

                ChapterSelector.RestartChapter();
                return;
            }
        }
    }

    private static FastResetAction ResolveAction()
    {
        if (Input.GetKey(FastResetConfig.NewGameKey.Value))
            return FastResetAction.NewGame;
        if (Input.GetKey(FastResetConfig.PreviousChapterKey.Value))
            return FastResetAction.PreviousChapter;
        if (Input.GetKey(FastResetConfig.NextChapterKey.Value))
            return FastResetAction.NextChapter;

        return FastResetAction.RestartChapter;
    }

    private static string BuildNotificationMessage(FastResetAction action, float secondsRemaining)
    {
        var prefix = $"Fast reset in {secondsRemaining:F0} s";

        switch (action)
        {
            case FastResetAction.PreviousChapter:
            {
                var previousChapter = ChapterResolver.ResolvePreviousChapter(ChapterSelector.CurrentChapter);
                return previousChapter.IsPlayable()
                    ? $"{prefix}: load previous chapter '{previousChapter.ToDisplayName()}'"
#if DEBUG
                    : $"{prefix}: chapter '{previousChapter.ToDisplayName()}' is not valid";
#else
                    : string.Empty;
#endif
            }
            case FastResetAction.NextChapter:
            {
                var nextChapter = ChapterResolver.ResolveNextChapter(ChapterSelector.CurrentChapter);
                return nextChapter.IsPlayable()
                    ? $"{prefix}: load next chapter '{nextChapter.ToDisplayName()}'"
#if DEBUG
                    : $"{prefix}: chapter '{nextChapter.ToDisplayName()}' is not valid";
#else
                    : string.Empty;
#endif
            }
            case FastResetAction.NewGame:
                return $"{prefix}: start new game";
            case FastResetAction.RestartChapter:
            default:
                return ChapterSelector.CurrentChapter.IsPlayable()
                    ? $"{prefix}: restart chapter '{ChapterSelector.CurrentChapter.ToDisplayName()}'"
#if DEBUG
                    : $"{prefix}: chapter '{ChapterSelector.CurrentChapter.ToDisplayName()}' is not valid";
#else
                    : string.Empty;
#endif
        }
    }
}
