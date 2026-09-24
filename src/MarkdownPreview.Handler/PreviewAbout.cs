using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarkdownPreview.Handler;

internal static class PreviewAbout
{
    public const string Author = "大白菜";
    public const string HelpUrl = "https://gitee.com/ouriping/jinghua-markdown-viewer/blob/master/docs/说明文档.md";
    public const string ReleasesUrl = "https://gitee.com/ouriping/jinghua-markdown-viewer/releases";

    private const string LatestReleaseApi =
        "https://gitee.com/api/v5/repos/ouriping/jinghua-markdown-viewer/releases/latest";

    private const string TagsApi =
        "https://gitee.com/api/v5/repos/ouriping/jinghua-markdown-viewer/tags?page=1&per_page=1";

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);

    public static string VersionText
    {
        get
        {
            var version = CurrentVersion;
            return version.Build <= 0 && version.Revision <= 0
                ? $"v{version.Major}.{version.Minor}"
                : $"v{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
        }
    }

    public static async Task<UpdateCheckResult> CheckUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.UserAgent] = "JinghuaMarkdownViewer-Preview";

                var release = await GetAsync(client, LatestReleaseApi, cancellationToken).ConfigureAwait(false);
                var tag = release == null ? null : ExtractJsonString(release, "tag_name");
                if (string.IsNullOrWhiteSpace(tag))
                {
                    var tags = await GetAsync(client, TagsApi, cancellationToken).ConfigureAwait(false);
                    tag = tags == null ? null : ExtractJsonString(tags, "name");
                }

                if (string.IsNullOrWhiteSpace(tag))
                {
                    return UpdateCheckResult.NoRelease();
                }

                return IsNewer(tag, CurrentVersion)
                    ? UpdateCheckResult.Available(tag.Trim())
                    : UpdateCheckResult.UpToDate();
            }
        }
        catch (Exception ex)
        {
            HandlerLog.Error("检查更新失败。", ex);
            return UpdateCheckResult.Failed();
        }
    }

    private static async Task<string> GetAsync(WebClient client, string url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
        }
        catch (WebException)
        {
            return null;
        }
    }

    private static bool IsNewer(string remoteTag, Version local)
    {
        var text = remoteTag.Trim().TrimStart('v', 'V');
        var cut = text.IndexOfAny(new[] { '-', '+', ' ' });
        if (cut > 0)
        {
            text = text.Substring(0, cut);
        }

        var parts = text.Split('.');
        if (parts.Length == 2)
        {
            text += ".0";
        }
        else if (parts.Length == 1)
        {
            text += ".0.0";
        }

        return Version.TryParse(text, out var remote) && remote > new Version(local.Major, local.Minor, Math.Max(local.Build, 0));
    }

    private static string ExtractJsonString(string json, string property)
    {
        var marker = "\"" + property + "\"";
        var index = json.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        var colon = json.IndexOf(':', index + marker.Length);
        if (colon < 0)
        {
            return null;
        }

        var quote = json.IndexOf('"', colon + 1);
        if (quote < 0)
        {
            return null;
        }

        var sb = new System.Text.StringBuilder();
        for (var i = quote + 1; i < json.Length; i++)
        {
            var ch = json[i];
            if (ch == '\\' && i + 1 < json.Length)
            {
                sb.Append(json[i + 1]);
                i++;
                continue;
            }

            if (ch == '"')
            {
                return sb.ToString();
            }

            sb.Append(ch);
        }

        return null;
    }
}

internal sealed class UpdateCheckResult
{
    public UpdateState State { get; private set; }
    public string RemoteTag { get; private set; }

    public static UpdateCheckResult UpToDate() => new UpdateCheckResult { State = UpdateState.UpToDate };
    public static UpdateCheckResult NoRelease() => new UpdateCheckResult { State = UpdateState.NoRelease };
    public static UpdateCheckResult Failed() => new UpdateCheckResult { State = UpdateState.Failed };
    public static UpdateCheckResult Available(string tag) => new UpdateCheckResult { State = UpdateState.UpdateAvailable, RemoteTag = tag };
}

internal enum UpdateState
{
    Checking,
    UpToDate,
    UpdateAvailable,
    NoRelease,
    Failed
}
