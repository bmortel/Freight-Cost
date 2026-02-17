using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Freight_Cost.Core;

/// <summary>
/// Handles update discovery + download from GitHub releases.
/// Keep this class UI-free so forms can call it from different screens.
/// </summary>
internal static class AppUpdater
{
    private const string Owner = "bmortel";
    private const string Repository = "Freight-Cost";
    private static readonly HttpClient HttpClient = CreateHttpClient();

    /// <summary>
    /// Represents one downloadable file attached to a GitHub release.
    /// </summary>
    internal sealed class UpdateAsset
    {
        internal required string Name { get; init; }
        internal required string DownloadUrl { get; init; }
    }

    /// <summary>
    /// Result returned to the UI after checking GitHub.
    /// </summary>
    internal sealed class UpdateCheckResult
    {
        internal required bool HasUpdate { get; init; }
        internal Version? LatestVersion { get; init; }
        internal string? LatestTag { get; init; }
        internal UpdateAsset? Asset { get; init; }
    }

    /// <summary>
    /// Compares current app version against latest GitHub release.
    /// </summary>
    internal static async Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0);
        var latestRelease = await GetLatestReleaseAsync(cancellationToken).ConfigureAwait(false);

        if (latestRelease.Version is null || latestRelease.Version <= currentVersion)
        {
            return new UpdateCheckResult
            {
                HasUpdate = false,
                LatestVersion = latestRelease.Version,
                LatestTag = latestRelease.Tag,
                Asset = null
            };
        }

        var selectedAsset = SelectPreferredAsset(latestRelease.Assets);
        return new UpdateCheckResult
        {
            HasUpdate = selectedAsset is not null,
            LatestVersion = latestRelease.Version,
            LatestTag = latestRelease.Tag,
            Asset = selectedAsset
        };
    }

    /// <summary>
    /// Downloads the selected release asset to a local file path.
    /// </summary>
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

    /// <summary>
    /// Calls GitHub's latest-release endpoint and extracts tag + asset list.
    /// </summary>
    private static async Task<LatestRelease> GetLatestReleaseAsync(CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync($"repos/{Owner}/{Repository}/releases/latest", cancellationToken)
            .ConfigureAwait(false);

        // GitHub returns 404 for this endpoint when a repository has no published releases yet.
        // In that case, fall back to the newest git tag so update checks still work.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return await GetLatestFromTagsAsync(cancellationToken).ConfigureAwait(false);
        }

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

    private static async Task<LatestRelease> GetLatestFromTagsAsync(CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync($"repos/{Owner}/{Repository}/tags?per_page=1", cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
        {
            return new LatestRelease
            {
                Tag = string.Empty,
                Version = null,
                Assets = Array.Empty<UpdateAsset>()
            };
        }

        var tag = document.RootElement[0].GetProperty("name").GetString() ?? string.Empty;
        return new LatestRelease
        {
            Tag = tag,
            Version = ParseVersion(tag),
            Assets = Array.Empty<UpdateAsset>()
        };
    }

    /// <summary>
    /// Picks the most installer-friendly file first.
    /// </summary>
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

    /// <summary>
    /// Converts a Git tag like "v1.2.3" into a Version object.
    /// </summary>
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

    /// <summary>
    /// Creates one shared HttpClient with GitHub-required headers.
    /// </summary>
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

    /// <summary>
    /// Internal DTO used while parsing the GitHub API response.
    /// </summary>
    private sealed class LatestRelease
    {
        internal required string Tag { get; init; }
        internal required Version? Version { get; init; }
        internal required IReadOnlyList<UpdateAsset> Assets { get; init; }
    }
}
