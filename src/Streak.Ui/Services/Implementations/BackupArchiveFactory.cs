namespace Streak.Ui.Services.Implementations;

public sealed class BackupArchiveFactory(
    IAppStoragePathService appStoragePathService,
    ICheckinProofFileStore checkinProofFileStore)
    : IBackupArchiveFactory
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly ICheckinProofFileStore _checkinProofFileStore = checkinProofFileStore;

    public Task<BackupArchiveArtifact> CreateManualBackupAsync(CancellationToken cancellationToken = default)
    {
        return CreateBackupAsync(
            () => DataBackupArchiveUtility.CreateBackupFilePath(_appStoragePathService.ExportDirectoryPath),
            cancellationToken);
    }

    public Task<BackupArchiveArtifact> CreateAutomatedBackupAsync(CancellationToken cancellationToken = default)
    {
        return CreateBackupAsync(
            () => DataBackupArchiveUtility.CreateAutomatedBackupFilePath(_appStoragePathService.ExportDirectoryPath),
            cancellationToken);
    }

    private async Task<BackupArchiveArtifact> CreateBackupAsync(
        Func<string> createBackupFilePath,
        CancellationToken cancellationToken)
    {
        var sourceDatabasePath = _appStoragePathService.DatabasePath;
        if (!File.Exists(sourceDatabasePath))
            throw new FileNotFoundException("The local Streak database could not be found.", sourceDatabasePath);

        var backupFilePath = createBackupFilePath();
        var unavailableReferencedProofPaths = await DataBackupArchiveUtility.CreateBackupAsync(
            sourceDatabasePath,
            _checkinProofFileStore,
            backupFilePath,
            cancellationToken);

        return new BackupArchiveArtifact
        {
            WorkingFilePath = backupFilePath,
            UnavailableReferencedProofPaths = unavailableReferencedProofPaths
        };
    }
}
