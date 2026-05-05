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

        _logger.LogInformation(
            "Nightly automated backup run starting. Local enabled: {LocalEnabled}. Cloud enabled: {CloudEnabled}.",
            localEnabled,
            cloudEnabled);

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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                localFailure = exception;
                _logger.LogWarning(
                    exception,
                    "Nightly local automated backup failed. Failure type: {FailureType}. Message: {FailureMessage}.",
                    exception.GetType().FullName,
                    exception.Message);
            }
        }

        if (cloudEnabled)
        {
            try
            {
                await _automatedCloudBackupService.UploadAutomatedBackupAsync(cancellationToken);
                cloudSucceeded = true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OneDriveBackupException exception)
            {
                cloudFailure = exception;
                if (exception.FailureKind == OneDriveBackupFailureKind.AuthRequired)
                    DisableAutomatedCloudBackupsAfterReconnectRequiredFailure();

                _logger.LogWarning(
                    exception,
                    "Nightly automated OneDrive backup failed with failure kind {FailureKind}. Message: {FailureMessage}.",
                    exception.FailureKind,
                    exception.Message);
            }
            catch (Exception exception)
            {
                cloudFailure = exception;
                _logger.LogError(
                    exception,
                    "Nightly automated OneDrive backup failed unexpectedly. Failure type: {FailureType}. Message: {FailureMessage}.",
                    exception.GetType().FullName,
                    exception.Message);
            }
        }

        var runResult = new AutomatedBackupRunResult
        {
            LocalEnabled = localEnabled,
            LocalSucceeded = localSavedLocation is not null,
            LocalSavedLocation = localSavedLocation,
            LocalFailure = localFailure,
            CloudEnabled = cloudEnabled,
            CloudSucceeded = cloudSucceeded,
            CloudFailureKind = cloudFailure is OneDriveBackupException oneDriveBackupException
                ? oneDriveBackupException.FailureKind
                : cloudFailure is null ? null : OneDriveBackupFailureKind.Unknown,
            CloudFailure = cloudFailure
        };

        if (runResult.HasAnyFailure)
        {
            _logger.LogWarning(
                "Nightly automated backup run finished with failures. Local enabled: {LocalEnabled}. Local succeeded: {LocalSucceeded}. Local saved path: {LocalSavedPath}. Cloud enabled: {CloudEnabled}. Cloud succeeded: {CloudSucceeded}. Cloud failure kind: {CloudFailureKind}.",
                runResult.LocalEnabled,
                runResult.LocalSucceeded,
                runResult.LocalSavedLocation?.SavedFileDisplayPath,
                runResult.CloudEnabled,
                runResult.CloudSucceeded,
                runResult.CloudFailureKind);
        }
        else
        {
            _logger.LogInformation(
                "Nightly automated backup run finished successfully. Local enabled: {LocalEnabled}. Local succeeded: {LocalSucceeded}. Local saved path: {LocalSavedPath}. Cloud enabled: {CloudEnabled}. Cloud succeeded: {CloudSucceeded}.",
                runResult.LocalEnabled,
                runResult.LocalSucceeded,
                runResult.LocalSavedLocation?.SavedFileDisplayPath,
                runResult.CloudEnabled,
                runResult.CloudSucceeded);
        }

        return runResult;
    }

    #region Private Helper Methods

    private void DisableAutomatedCloudBackupsAfterReconnectRequiredFailure()
    {
        try
        {
            _automatedBackupConfigurationService.SetIsCloudEnabled(false);
            _logger.LogInformation(
                "Disabled nightly automated OneDrive backups after a reconnect-required auth failure so the app does not repeat the same cloud backup failure until the user reconnects OneDrive.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to disable nightly automated OneDrive backups after a reconnect-required auth failure.");
        }
    }

    #endregion
}
