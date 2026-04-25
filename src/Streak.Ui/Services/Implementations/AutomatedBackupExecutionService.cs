namespace Streak.Ui.Services.Implementations;

public sealed class AutomatedBackupExecutionService(
    IBackupArchiveFactory backupArchiveFactory,
    IAutomatedBackupFileSaver automatedBackupFileSaver,
    ILogger<AutomatedBackupExecutionService> logger)
    : IAutomatedBackupExecutionService
{
    private readonly IBackupArchiveFactory _backupArchiveFactory = backupArchiveFactory;
    private readonly IAutomatedBackupFileSaver _automatedBackupFileSaver = automatedBackupFileSaver;
    private readonly ILogger<AutomatedBackupExecutionService> _logger = logger;

    public async Task<SavedFileLocation> ExecuteAutomatedBackupAsync(CancellationToken cancellationToken = default)
    {
        using var backupArchive = await _backupArchiveFactory.CreateAutomatedBackupAsync(cancellationToken);

        try
        {
            if (backupArchive.UnavailableReferencedProofPaths.Count > 0)
            {
                _logger.LogWarning(
                    "Automated backup skipped {UnavailableProofFileCount} unavailable picture proof reference(s): {@UnavailableProofPaths}",
                    backupArchive.UnavailableReferencedProofPaths.Count,
                    backupArchive.UnavailableReferencedProofPaths);
            }

            return await _automatedBackupFileSaver.SaveBackupAsync(
                backupArchive.WorkingFilePath,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Automated backup execution failed.");
            throw;
        }
    }
}
