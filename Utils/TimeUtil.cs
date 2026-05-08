using UnityEngine;

namespace SpeedrunMod.Utils;

public class TimeUtil
{
    private const float TimescaleThreshold = 3000f;

    public static void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }

    public static void MultiplyTimeScale(float multiplier)
    {
        var hz = Screen.currentResolution.refreshRate;
        if (hz <= 0f)
            hz = 60;

        var candidate = Time.timeScale * multiplier;
        var maxScale = TimescaleThreshold / hz;
        Time.timeScale = Mathf.Min(candidate, maxScale);
    }
}
