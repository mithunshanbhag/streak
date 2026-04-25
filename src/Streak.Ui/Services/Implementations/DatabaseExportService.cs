namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseExportService(
    IBackupArchiveFactory backupArchiveFactory,
    IDatabaseExportFileSaver databaseExportFileSaver,
    IManualBackupStatusStore manualBackupStatusStore,
    TimeProvider timeProvider,
    ILogger<DatabaseExportService> logger)
    : IDatabaseExportService
{
    private readonly IBackupArchiveFactory _backupArchiveFactory = backupArchiveFactory;
    private readonly IDatabaseExportFileSaver _databaseExportFileSaver = databaseExportFileSaver;
    private readonly IManualBackupStatusStore _manualBackupStatusStore = manualBackupStatusStore;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<DatabaseExportService> _logger = logger;

    public async Task<DatabaseExportResult> ExportDatabaseAsync(CancellationToken cancellationToken = default)
    {
        using var backupArchive = await _backupArchiveFactory.CreateManualBackupAsync(cancellationToken);

        try
        {
            if (backupArchive.UnavailableReferencedProofPaths.Count > 0)
            {
                _logger.LogWarning(
                    "Database export skipped {UnavailableProofFileCount} unavailable picture proof reference(s): {@UnavailableProofPaths}",
                    backupArchive.UnavailableReferencedProofPaths.Count,
                    backupArchive.UnavailableReferencedProofPaths);
            }

            var exportResult = await _databaseExportFileSaver.SaveBackupAsync(
                backupArchive.WorkingFilePath,
                cancellationToken);

            if (exportResult.Status == DatabaseExportStatus.Saved)
            {
                _manualBackupStatusStore.SetLastSuccessfulBackupUtc(
                    ManualBackupLocation.Local,
                    _timeProvider.GetUtcNow());
            }

            return exportResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database export failed.");
            throw;
        }
    }
}
