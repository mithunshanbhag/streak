namespace Streak.Ui.Services.Implementations;

public sealed class AutomatedBackupExecutionService(
    IAppStoragePathService appStoragePathService,
    IAutomatedBackupFileSaver automatedBackupFileSaver,
    ILogger<AutomatedBackupExecutionService> logger)
    : IAutomatedBackupExecutionService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IAutomatedBackupFileSaver _automatedBackupFileSaver = automatedBackupFileSaver;
    private readonly ILogger<AutomatedBackupExecutionService> _logger = logger;

    public async Task<SavedFileLocation> ExecuteAutomatedBackupAsync(CancellationToken cancellationToken = default)
    {
        var sourceDatabasePath = _appStoragePathService.DatabasePath;
        if (!File.Exists(sourceDatabasePath))
            throw new FileNotFoundException("The local Streak database could not be found.", sourceDatabasePath);

        var workingBackupPath = DataBackupArchiveUtility.CreateAutomatedBackupFilePath(_appStoragePathService.ExportDirectoryPath);

        try
        {
            var unavailableReferencedProofPaths = await DataBackupArchiveUtility.CreateBackupAsync(
                sourceDatabasePath,
                _appStoragePathService.CheckinProofsDirectoryPath,
                workingBackupPath,
                cancellationToken);

            if (unavailableReferencedProofPaths.Count > 0)
            {
                _logger.LogWarning(
                    "Automated backup skipped {UnavailableProofFileCount} unavailable picture proof reference(s) for {DatabasePath}: {@UnavailableProofPaths}",
                    unavailableReferencedProofPaths.Count,
                    sourceDatabasePath,
                    unavailableReferencedProofPaths);
            }

            return await _automatedBackupFileSaver.SaveBackupAsync(
                workingBackupPath,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Automated backup execution failed for {DatabasePath}.", sourceDatabasePath);
            throw;
        }
        finally
        {
            DataBackupArchiveUtility.DeleteBackupIfExists(workingBackupPath);
        }
    }
}
