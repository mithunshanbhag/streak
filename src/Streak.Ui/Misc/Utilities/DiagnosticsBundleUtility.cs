using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Streak.Ui.Misc.Utilities;

internal static class DiagnosticsBundleUtility
{
    private const string DiagnosticsBundleSearchPattern = "streak-diagnostics-*.zip";

    public static string CreateBundleFilePath(string exportDirectoryPath, TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportDirectoryPath);
        ArgumentNullException.ThrowIfNull(timeProvider);

        Directory.CreateDirectory(exportDirectoryPath);

        var timestamp = timeProvider.GetLocalNow().ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        return Path.Combine(exportDirectoryPath, $"streak-diagnostics-{timestamp}.zip");
    }

    public static async Task CreateBundleAsync(
        string diagnosticsDirectoryPath,
        string bundleFilePath,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bundleFilePath);
        ArgumentNullException.ThrowIfNull(timeProvider);

        DeleteBundleIfExists(bundleFilePath);

        await using var bundleStream = new FileStream(
            bundleFilePath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None);
        using var zipArchive = new ZipArchive(bundleStream, ZipArchiveMode.Create, leaveOpen: false);

        foreach (var diagnosticsFilePath in EnumerateDiagnosticsArtifactPaths(diagnosticsDirectoryPath))
            await AddDiagnosticsArtifactAsync(
                zipArchive,
                diagnosticsFilePath,
                cancellationToken);

        await AddManifestAsync(
            zipArchive,
            diagnosticsDirectoryPath,
            timeProvider,
            cancellationToken);
    }

    public static void DeleteBundleIfExists(string bundleFilePath)
    {
        if (File.Exists(bundleFilePath))
            File.Delete(bundleFilePath);
    }

    public static void DeleteCachedBundles(string exportDirectoryPath)
    {
        if (!Directory.Exists(exportDirectoryPath))
            return;

        foreach (var bundleFilePath in Directory.EnumerateFiles(exportDirectoryPath, DiagnosticsBundleSearchPattern, SearchOption.TopDirectoryOnly))
            File.Delete(bundleFilePath);
    }

    private static IEnumerable<string> EnumerateDiagnosticsArtifactPaths(string diagnosticsDirectoryPath)
    {
        if (!Directory.Exists(diagnosticsDirectoryPath))
            return [];

        return Directory
            .EnumerateFiles(diagnosticsDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(filePath => !string.Equals(Path.GetExtension(filePath), ".db", StringComparison.OrdinalIgnoreCase))
            .OrderBy(filePath => filePath, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task AddDiagnosticsArtifactAsync(
        ZipArchive zipArchive,
        string diagnosticsFilePath,
        CancellationToken cancellationToken)
    {
        var entry = zipArchive.CreateEntry(
            Path.Combine("logs", Path.GetFileName(diagnosticsFilePath)).Replace('\\', '/'),
            CompressionLevel.Optimal);

        await using var sourceStream = new FileStream(
            diagnosticsFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        await using var entryStream = entry.Open();
        await sourceStream.CopyToAsync(entryStream, cancellationToken);
    }

    private static async Task AddManifestAsync(
        ZipArchive zipArchive,
        string diagnosticsDirectoryPath,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var manifestEntry = zipArchive.CreateEntry("manifest.json", CompressionLevel.Optimal);
        await using var entryStream = manifestEntry.Open();

        var manifest = new DiagnosticsManifest
        {
            AppVersion = typeof(DiagnosticsBundleUtility).Assembly.GetName().Version?.ToString() ?? "unknown",
            Platform = GetPlatformName(),
            OsVersion = Environment.OSVersion.VersionString,
            ExportedAtUtc = timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture),
            StructuredLogDirectory = diagnosticsDirectoryPath
        };

        await JsonSerializer.SerializeAsync(
            entryStream,
            manifest,
            DiagnosticsManifestJsonContext.Default.DiagnosticsManifest,
            cancellationToken);
    }

    private static string GetPlatformName()
    {
#if WINDOWS
        return "Windows";
#elif ANDROID
        return "Android";
#else
        return Environment.OSVersion.Platform.ToString();
#endif
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DiagnosticsManifest))]
internal sealed partial class DiagnosticsManifestJsonContext : JsonSerializerContext;

internal sealed class DiagnosticsManifest
{
    public required string AppVersion { get; init; }

    public required string Platform { get; init; }

    public required string OsVersion { get; init; }

    public required string ExportedAtUtc { get; init; }

    public required string StructuredLogDirectory { get; init; }
}
