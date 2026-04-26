namespace Streak.Ui.Services.Implementations;

public sealed class AutomatedCloudBackupService(
    IBackupArchiveFactory backupArchiveFactory,
    IOneDriveBackupUploadClient oneDriveBackupUploadClient,
    ILogger<AutomatedCloudBackupService> logger)
    : IAutomatedCloudBackupService
{
    private readonly IBackupArchiveFactory _backupArchiveFactory = backupArchiveFactory;
    private readonly IOneDriveBackupUploadClient _oneDriveBackupUploadClient = oneDriveBackupUploadClient;
    private readonly ILogger<AutomatedCloudBackupService> _logger = logger;

    public async Task UploadAutomatedBackupAsync(CancellationToken cancellationToken = default)
    {
        using var backupArchive = await _backupArchiveFactory.CreateAutomatedBackupAsync(cancellationToken);

        if (backupArchive.UnavailableReferencedProofPaths.Count > 0)
        {
            _logger.LogWarning(
                "Automated OneDrive backup skipped {UnavailableProofFileCount} unavailable picture proof reference(s): {@UnavailableProofPaths}",
                backupArchive.UnavailableReferencedProofPaths.Count,
                backupArchive.UnavailableReferencedProofPaths);
        }

        try
        {
            await _oneDriveBackupUploadClient.UploadAutomatedBackupAsync(
                backupArchive.WorkingFilePath,
                backupArchive.FileName,
                cancellationToken);
        }
        catch (OneDriveBackupException exception)
        {
            _logger.LogWarning(
                exception,
                "Automated OneDrive backup failed with failure kind {FailureKind}.",
                exception.FailureKind);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Automated OneDrive backup failed unexpectedly.");
            throw;
        }
    }
}
