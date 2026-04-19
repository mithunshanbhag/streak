namespace Streak.Ui.Services.Implementations;

public sealed class ManualBackupCompletionNotifier(
    ISnackbar snackbar,
    IBackupFolderOpener backupFolderOpener)
    : IManualBackupCompletionNotifier
{
    private readonly IBackupFolderOpener _backupFolderOpener = backupFolderOpener;
    private readonly ISnackbar _snackbar = snackbar;

    public void NotifyCompleted(DatabaseExportResult exportResult)
    {
        ArgumentNullException.ThrowIfNull(exportResult);

        if (exportResult.Status != DatabaseExportStatus.Saved || exportResult.SavedFileLocation is null)
            throw new InvalidOperationException("A saved backup location is required to notify a completed export.");

        var savedFileLocation = exportResult.SavedFileLocation;

        _snackbar.Add(
            $"Backup saved to {savedFileLocation.ParentFolderDisplayPath}.",
            MudBlazor.Severity.Success,
            options =>
            {
                if (!_backupFolderOpener.CanOpenFolder(BackupFolderKind.ManualExport, savedFileLocation))
                    return;

                options.Action = OpenFolderActionText;
                options.ActionColor = MudBlazor.Color.Primary;
                options.RequireInteraction = true;
                options.OnClick = _ =>
                {
                    _backupFolderOpener.OpenFolder(BackupFolderKind.ManualExport, savedFileLocation);
                    return Task.CompletedTask;
                };
            },
            key: $"manual-backup:{savedFileLocation.SavedFileDisplayPath}");
    }

    private const string OpenFolderActionText = "Open folder";
}
