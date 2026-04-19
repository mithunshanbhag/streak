#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDiagnosticsExportFileSaver : IDiagnosticsExportFileSaver
{
    private const string DiagnosticsBundleMimeType = "application/zip";

    public async Task<DiagnosticsExportResult> SaveBundleAsync(
        string bundleFilePath,
        CancellationToken cancellationToken = default)
    {
        _ = await AndroidMediaStoreBackupFileWriter.SaveFileAsync(
            bundleFilePath,
            Environment.DirectoryDownloads,
            DiagnosticsBundleMimeType,
            cancellationToken);

        return DiagnosticsExportResult.Saved;
    }
}
#endif
