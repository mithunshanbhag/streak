#if WINDOWS
namespace Streak.Ui.Services.Implementations;

public sealed class WindowsDiagnosticsExportFileSaver : IDiagnosticsExportFileSaver
{
    public async Task<DiagnosticsExportResult> SaveBundleAsync(
        string bundleFilePath,
        CancellationToken cancellationToken = default)
    {
        var wasSaved = await WindowsFileSavePickerExportUtility.SaveFileAsync(
            bundleFilePath,
            "Diagnostics support bundle",
            ".zip",
            cancellationToken);

        return wasSaved
            ? DiagnosticsExportResult.Saved
            : DiagnosticsExportResult.Cancelled;
    }
}
#endif
