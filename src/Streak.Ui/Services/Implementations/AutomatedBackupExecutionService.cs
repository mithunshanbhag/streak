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

        var workingBackupPath = DatabaseBackupFileUtility.CreateAutomatedBackupFilePath(_appStoragePathService.ExportDirectoryPath);

        try
        {
            await DatabaseBackupFileUtility.CreateBackupAsync(sourceDatabasePath, workingBackupPath, cancellationToken);

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
            DatabaseBackupFileUtility.DeleteBackupIfExists(workingBackupPath);
        }
    }
}
