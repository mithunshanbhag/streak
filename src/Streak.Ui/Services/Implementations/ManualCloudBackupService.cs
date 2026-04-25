namespace Streak.Ui.Services.Implementations;

public sealed class ManualCloudBackupService(
    IBackupArchiveFactory backupArchiveFactory,
    IOneDriveBackupUploadClient oneDriveBackupUploadClient,
    IManualBackupStatusStore manualBackupStatusStore,
    TimeProvider timeProvider,
    ILogger<ManualCloudBackupService> logger)
    : IManualCloudBackupService
{
    private readonly IBackupArchiveFactory _backupArchiveFactory = backupArchiveFactory;
    private readonly IOneDriveBackupUploadClient _oneDriveBackupUploadClient = oneDriveBackupUploadClient;
    private readonly IManualBackupStatusStore _manualBackupStatusStore = manualBackupStatusStore;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<ManualCloudBackupService> _logger = logger;

    public async Task UploadManualBackupAsync(CancellationToken cancellationToken = default)
    {
        using var backupArchive = await _backupArchiveFactory.CreateManualBackupAsync(cancellationToken);

        if (backupArchive.UnavailableReferencedProofPaths.Count > 0)
        {
            _logger.LogWarning(
                "Manual OneDrive backup skipped {UnavailableProofFileCount} unavailable picture proof reference(s): {@UnavailableProofPaths}",
                backupArchive.UnavailableReferencedProofPaths.Count,
                backupArchive.UnavailableReferencedProofPaths);
        }

        try
        {
            await _oneDriveBackupUploadClient.UploadManualBackupAsync(
                backupArchive.WorkingFilePath,
                backupArchive.FileName,
                cancellationToken);

            _manualBackupStatusStore.SetLastSuccessfulBackupUtc(
                ManualBackupLocation.Cloud,
                _timeProvider.GetUtcNow());
        }
        catch (OneDriveBackupException exception)
        {
            _logger.LogWarning(
                exception,
                "Manual OneDrive backup failed with failure kind {FailureKind}.",
                exception.FailureKind);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Manual OneDrive backup failed unexpectedly.");
            throw;
        }
    }
}
