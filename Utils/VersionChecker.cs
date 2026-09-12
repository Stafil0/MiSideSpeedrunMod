using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeedrunMod.Utils;

internal static class VersionChecker
{
    internal const string RepoUrl = "https://github.com/SliceCraft/MiSideSpeedrunMod";
    internal const string LatestReleasePath = "/releases/latest";
    private const string TagPath = "/releases/tag/";

    internal static string LatestVersion { get; private set; }
    internal static string CurrentVersion => MyPluginInfo.PLUGIN_VERSION;
    internal static string DownloadUrl => RepoUrl + LatestReleasePath;
    internal static bool UpdateAvailable => IsNewerVersion(LatestVersion, CurrentVersion);

    internal static async void CheckForUpdatesAsync()
    {
        try
        {
            string latest = await GetLatestVersionAsync();
            if (string.IsNullOrEmpty(latest))
            {
                return;
            }

            LatestVersion = latest;
        }
        catch (Exception)
        {
            Plugin.Log.LogError("Unable to request version");
        }
    }

    private static async Task<string> GetLatestVersionAsync()
    {
        using HttpClientHandler handler = new() { AllowAutoRedirect = false };
        using HttpClient client = new(handler) { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"{MyPluginInfo.PLUGIN_NAME}/{CurrentVersion}");
        using HttpResponseMessage response = await client.GetAsync(DownloadUrl);
        int status = (int)response.StatusCode;
        if (status is < 300 or >= 400)
        {
            return null;
        }

        Uri location = response.Headers.Location;
        if (location == null)
        {
            return null;
        }

        if (!location.IsAbsoluteUri && response.RequestMessage?.RequestUri != null)
        {
            location = new Uri(response.RequestMessage.RequestUri, location);
        }

        return ParseTagFromLocation(location.ToString());
    }

    private static string ParseTagFromLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        int tagAt = location.IndexOf(TagPath, StringComparison.OrdinalIgnoreCase);
        if (tagAt < 0)
        {
            return null;
        }

        string tag = location[(tagAt + TagPath.Length)..].Trim().Trim('/');
        int slash = tag.IndexOf('/');
        if (slash >= 0)
        {
            tag = tag[..slash];
        }

        if (tag.Length == 0)
        {
            return null;
        }

        return StripLeadingV(tag);
    }

    private static bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        if (!TryParse(latestVersion, out Version latest) || !TryParse(currentVersion, out Version current))
        {
            return false;
        }

        return latest > current;
    }

    private static bool TryParse(string value, out Version version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string s = StripLeadingV(value.Trim());
        int cut = s.IndexOfAny(new[] { '-', '+' });
        if (cut >= 0)
        {
            s = s[..cut];
        }

        return Version.TryParse(s, out version);
    }

    private static string StripLeadingV(string tag)
    {
        if (tag.StartsWith("v", StringComparison.OrdinalIgnoreCase) && tag.Length > 1 && char.IsDigit(tag[1]))
        {
            return tag[1..];
        }

        return tag;
    }
}
