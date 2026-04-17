using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Streak.Ui.Services.Implementations;

public sealed class DiagnosticsExportService(
    IAppStoragePathService appStoragePathService,
    IDiagnosticsExportFileSaver diagnosticsExportFileSaver,
    TimeProvider timeProvider,
    ILogger<DiagnosticsExportService> logger)
    : IDiagnosticsExportService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IDiagnosticsExportFileSaver _diagnosticsExportFileSaver = diagnosticsExportFileSaver;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<DiagnosticsExportService> _logger = logger;

    public async Task<DiagnosticsExportResult> ExportDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var bundleFilePath = CreateBundleFilePath(_appStoragePathService.ExportDirectoryPath);

        try
        {
            await CreateBundleAsync(bundleFilePath, cancellationToken);

            return await _diagnosticsExportFileSaver.SaveBundleAsync(
                bundleFilePath,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Diagnostics export failed for {DiagnosticsDirectoryPath}.",
                _appStoragePathService.DiagnosticsDirectoryPath);
            throw;
        }
        finally
        {
            DeleteBundleIfExists(bundleFilePath);
        }
    }

    private async Task CreateBundleAsync(string bundleFilePath, CancellationToken cancellationToken)
    {
        DeleteBundleIfExists(bundleFilePath);

        await using var bundleStream = new FileStream(
            bundleFilePath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None);
        using var zipArchive = new ZipArchive(bundleStream, ZipArchiveMode.Create, leaveOpen: false);

        foreach (var diagnosticsFilePath in EnumerateDiagnosticsArtifactPaths(_appStoragePathService.DiagnosticsDirectoryPath))
            await AddDiagnosticsArtifactAsync(
                zipArchive,
                diagnosticsFilePath,
                cancellationToken);

        await AddManifestAsync(zipArchive, cancellationToken);
    }

    private IEnumerable<string> EnumerateDiagnosticsArtifactPaths(string diagnosticsDirectoryPath)
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

    private async Task AddManifestAsync(ZipArchive zipArchive, CancellationToken cancellationToken)
    {
        var manifestEntry = zipArchive.CreateEntry("manifest.json", CompressionLevel.Optimal);
        await using var entryStream = manifestEntry.Open();

        var manifest = new DiagnosticsManifest
        {
            AppVersion = typeof(DiagnosticsExportService).Assembly.GetName().Version?.ToString() ?? "unknown",
            Platform = GetPlatformName(),
            OsVersion = Environment.OSVersion.VersionString,
            ExportedAtUtc = _timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture),
            StructuredLogDirectory = _appStoragePathService.DiagnosticsDirectoryPath
        };

        await JsonSerializer.SerializeAsync(
            entryStream,
            manifest,
            DiagnosticsManifestJsonContext.Default.DiagnosticsManifest,
            cancellationToken);
    }

    private string CreateBundleFilePath(string exportDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportDirectoryPath);

        Directory.CreateDirectory(exportDirectoryPath);

        var timestamp = _timeProvider.GetLocalNow().ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        return Path.Combine(exportDirectoryPath, $"streak-diagnostics-{timestamp}.zip");
    }

    private static void DeleteBundleIfExists(string bundleFilePath)
    {
        if (File.Exists(bundleFilePath))
            File.Delete(bundleFilePath);
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
