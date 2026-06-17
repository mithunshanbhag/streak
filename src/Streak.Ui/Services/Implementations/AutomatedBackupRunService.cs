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

    public async Task<AutomatedBackupRunResult> ExecuteEnabledLocalBackupAsync(CancellationToken cancellationToken = default)
    {
        var localEnabled = _automatedBackupConfigurationService.GetIsEnabled();
        if (!localEnabled)
            return new AutomatedBackupRunResult();

        _logger.LogInformation(
            "Nightly local automated backup run starting. Local enabled: {LocalEnabled}.",
            localEnabled);

        var runResult = await ExecuteLocalBackupCoreAsync(localEnabled, cancellationToken);

        LogLocalRunCompletion(runResult);
        return runResult;
    }

    public async Task<AutomatedBackupRunResult> ExecuteEnabledCloudBackupAsync(CancellationToken cancellationToken = default)
    {
        var cloudEnabled = _automatedBackupConfigurationService.GetIsCloudEnabled();
        if (!cloudEnabled)
            return new AutomatedBackupRunResult();

        _logger.LogInformation(
            "Nightly automated OneDrive backup run starting. Cloud enabled: {CloudEnabled}.",
            cloudEnabled);

        var runResult = await ExecuteCloudBackupCoreAsync(cloudEnabled, cancellationToken);

        LogCloudRunCompletion(runResult);
        return runResult;
    }

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

        var localResult = await ExecuteLocalBackupCoreAsync(localEnabled, cancellationToken);
        var cloudResult = await ExecuteCloudBackupCoreAsync(cloudEnabled, cancellationToken);
        var runResult = MergeResults(localResult, cloudResult);

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

    private async Task<AutomatedBackupRunResult> ExecuteLocalBackupCoreAsync(
        bool localEnabled,
        CancellationToken cancellationToken)
    {
        if (!localEnabled)
            return new AutomatedBackupRunResult();

        SavedFileLocation? localSavedLocation = null;
        Exception? localFailure = null;

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

        return new AutomatedBackupRunResult
        {
            LocalEnabled = localEnabled,
            LocalSucceeded = localSavedLocation is not null,
            LocalSavedLocation = localSavedLocation,
            LocalFailure = localFailure
        };
    }

    private async Task<AutomatedBackupRunResult> ExecuteCloudBackupCoreAsync(
        bool cloudEnabled,
        CancellationToken cancellationToken)
    {
        if (!cloudEnabled)
            return new AutomatedBackupRunResult();

        Exception? cloudFailure = null;
        var cloudSucceeded = false;

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

        return new AutomatedBackupRunResult
        {
            CloudEnabled = cloudEnabled,
            CloudSucceeded = cloudSucceeded,
            CloudFailureKind = cloudFailure is OneDriveBackupException oneDriveBackupException
                ? oneDriveBackupException.FailureKind
                : cloudFailure is null ? null : OneDriveBackupFailureKind.Unknown,
            CloudFailure = cloudFailure
        };
    }

    private static AutomatedBackupRunResult MergeResults(
        AutomatedBackupRunResult localResult,
        AutomatedBackupRunResult cloudResult)
    {
        return new AutomatedBackupRunResult
        {
            LocalEnabled = localResult.LocalEnabled,
            LocalSucceeded = localResult.LocalSucceeded,
            LocalSavedLocation = localResult.LocalSavedLocation,
            LocalFailure = localResult.LocalFailure,
            CloudEnabled = cloudResult.CloudEnabled,
            CloudSucceeded = cloudResult.CloudSucceeded,
            CloudFailureKind = cloudResult.CloudFailureKind,
            CloudFailure = cloudResult.CloudFailure
        };
    }

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

    private void LogLocalRunCompletion(AutomatedBackupRunResult runResult)
    {
        if (runResult.LocalSucceeded)
        {
            _logger.LogInformation(
                "Nightly local automated backup run finished successfully. Local saved path: {LocalSavedPath}.",
                runResult.LocalSavedLocation?.SavedFileDisplayPath);
            return;
        }

        _logger.LogWarning(
            "Nightly local automated backup run finished with failure. Failure type: {FailureType}. Message: {FailureMessage}.",
            runResult.LocalFailure?.GetType().FullName,
            runResult.LocalFailure?.Message);
    }

    private void LogCloudRunCompletion(AutomatedBackupRunResult runResult)
    {
        if (runResult.CloudSucceeded)
        {
            _logger.LogInformation("Nightly automated OneDrive backup run finished successfully.");
            return;
        }

        _logger.LogWarning(
            "Nightly automated OneDrive backup run finished with failure. Cloud failure kind: {CloudFailureKind}. Failure type: {FailureType}. Message: {FailureMessage}.",
            runResult.CloudFailureKind,
            runResult.CloudFailure?.GetType().FullName,
            runResult.CloudFailure?.Message);
    }

    #endregion
}
