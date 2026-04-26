namespace Streak.Ui.Services.Implementations;

public sealed class AutomatedBackupRunService(
    IAutomatedBackupConfigurationService automatedBackupConfigurationService,
    IAutomatedBackupExecutionService automatedBackupExecutionService,
    IAutomatedCloudBackupService automatedCloudBackupService,
    ILogger<AutomatedBackupRunService> logger)
    : IAutomatedBackupRunService
{
    private readonly IAutomatedBackupConfigurationService _automatedBackupConfigurationService = automatedBackupConfigurationService;
    private readonly IAutomatedBackupExecutionService _automatedBackupExecutionService = automatedBackupExecutionService;
    private readonly IAutomatedCloudBackupService _automatedCloudBackupService = automatedCloudBackupService;
    private readonly ILogger<AutomatedBackupRunService> _logger = logger;

    public async Task<AutomatedBackupRunResult> ExecuteEnabledBackupsAsync(CancellationToken cancellationToken = default)
    {
        var localEnabled = _automatedBackupConfigurationService.GetIsEnabled();
        var cloudEnabled = _automatedBackupConfigurationService.GetIsCloudEnabled();

        if (!localEnabled && !cloudEnabled)
            return new AutomatedBackupRunResult();

        SavedFileLocation? localSavedLocation = null;
        Exception? localFailure = null;
        Exception? cloudFailure = null;
        var cloudSucceeded = false;

        if (localEnabled)
        {
            try
            {
                localSavedLocation = await _automatedBackupExecutionService.ExecuteAutomatedBackupAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                localFailure = exception;
                _logger.LogWarning(exception, "Nightly local automated backup failed.");
            }
        }

        if (cloudEnabled)
        {
            try
            {
                await _automatedCloudBackupService.UploadAutomatedBackupAsync(cancellationToken);
                cloudSucceeded = true;
            }
            catch (OneDriveBackupException exception)
            {
                cloudFailure = exception;
                _logger.LogWarning(
                    exception,
                    "Nightly automated OneDrive backup failed with failure kind {FailureKind}.",
                    exception.FailureKind);
            }
            catch (Exception exception)
            {
                cloudFailure = exception;
                _logger.LogError(exception, "Nightly automated OneDrive backup failed unexpectedly.");
            }
        }

        if (localFailure is not null && localSavedLocation is null && !cloudSucceeded)
            throw localFailure;

        if (cloudFailure is not null && localSavedLocation is null)
            throw cloudFailure;

        return new AutomatedBackupRunResult
        {
            LocalEnabled = localEnabled,
            LocalSucceeded = localSavedLocation is not null,
            LocalSavedLocation = localSavedLocation,
            CloudEnabled = cloudEnabled,
            CloudSucceeded = cloudSucceeded
        };
    }
}
