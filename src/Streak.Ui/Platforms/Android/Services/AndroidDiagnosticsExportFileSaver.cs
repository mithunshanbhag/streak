#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDiagnosticsExportFileSaver : IDiagnosticsExportFileSaver
{
    private const string DiagnosticsBundleMimeType = "application/zip";

    private static readonly string RelativeDirectoryPath = string.Join(
        '/',
        Environment.DirectoryDownloads,
        StreakExportStorageConstants.AndroidRootDirectoryName,
        StreakExportStorageConstants.DiagnosticsDirectoryName);

    public async Task<DiagnosticsExportResult> SaveBundleAsync(
        string bundleFilePath,
        CancellationToken cancellationToken = default)
    {
        _ = await AndroidMediaStoreBackupFileWriter.SaveFileAsync(
            bundleFilePath,
            RelativeDirectoryPath,
            DiagnosticsBundleMimeType,
            StreakExportStorageConstants.DiagnosticsDisplayDirectoryPath,
            cancellationToken);

        return DiagnosticsExportResult.Saved;
    }
}
#endif
