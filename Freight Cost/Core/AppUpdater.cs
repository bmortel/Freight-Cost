using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Freight_Cost.Core;

internal static class AppUpdater
{
    private const string Owner = "bmortel";
    private const string Repository = "Freight-Cost";
    private static readonly HttpClient HttpClient = CreateHttpClient();

    internal sealed class UpdateAsset
    {
        internal required string Name { get; init; }
        internal required string DownloadUrl { get; init; }
    }

    internal sealed class UpdateCheckResult
    {
        internal required bool HasUpdate { get; init; }
        internal required Version CurrentVersion { get; init; }
        internal Version? LatestVersion { get; init; }
        internal string? LatestTag { get; init; }
        internal UpdateAsset? Asset { get; init; }
    }

    internal static async Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
        var latestRelease = await GetLatestReleaseAsync(cancellationToken).ConfigureAwait(false);

        if (latestRelease.Version is null || latestRelease.Version <= currentVersion)
        {
            return new UpdateCheckResult
            {
                HasUpdate = false,
                CurrentVersion = currentVersion,
                LatestVersion = latestRelease.Version,
                LatestTag = latestRelease.Tag,
                Asset = null
            };
        }

        var selectedAsset = SelectPreferredAsset(latestRelease.Assets);
        return new UpdateCheckResult
        {
            HasUpdate = selectedAsset is not null,
            CurrentVersion = currentVersion,
            LatestVersion = latestRelease.Version,
            LatestTag = latestRelease.Tag,
            Asset = selectedAsset
        };
    }

    internal static async Task DownloadAssetAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var response = await HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var destinationStream = File.Create(destinationPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<LatestRelease> GetLatestReleaseAsync(CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync($"repos/{Owner}/{Repository}/releases/latest", cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        var root = document.RootElement;
        var tag = root.GetProperty("tag_name").GetString() ?? string.Empty;
        var version = ParseVersion(tag);

        var assets = new List<UpdateAsset>();
        if (root.TryGetProperty("assets", out var assetsNode) && assetsNode.ValueKind == JsonValueKind.Array)
        {
            foreach (var asset in assetsNode.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString();
                var downloadUrl = asset.GetProperty("browser_download_url").GetString();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(downloadUrl))
                {
                    continue;
                }

                assets.Add(new UpdateAsset
                {
                    Name = name,
                    DownloadUrl = downloadUrl
                });
            }
        }

        return new LatestRelease
        {
            Tag = tag,
            Version = version,
            Assets = assets
        };
    }

    private static UpdateAsset? SelectPreferredAsset(IReadOnlyList<UpdateAsset> assets)
    {
        foreach (var extension in new[] { ".exe", ".msi", ".zip" })
        {
            foreach (var asset in assets)
            {
                if (asset.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }
            }
        }

        return assets.Count > 0 ? assets[0] : null;
    }

    private static Version? ParseVersion(string? rawTag)
    {
        if (string.IsNullOrWhiteSpace(rawTag))
        {
            return null;
        }

        var normalized = rawTag.Trim().TrimStart('v', 'V');
        var prereleaseSplit = normalized.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);

        return Version.TryParse(prereleaseSplit[0], out var version) ? version : null;
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Freight-Cost-Updater/1.0");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return httpClient;
    }

    private sealed class LatestRelease
    {
        internal required string Tag { get; init; }
        internal required Version? Version { get; init; }
        internal required IReadOnlyList<UpdateAsset> Assets { get; init; }
    }
}
