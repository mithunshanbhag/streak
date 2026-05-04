namespace Streak.Ui.Services.Implementations;

public sealed class AutomatedBackupConfigurationService(
    IAppStoragePathService appStoragePathService,
    IAutomatedBackupScheduler automatedBackupScheduler,
    ILogger<AutomatedBackupConfigurationService> logger)
    : IAutomatedBackupConfigurationService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IAutomatedBackupScheduler _automatedBackupScheduler = automatedBackupScheduler;
    private readonly ILogger<AutomatedBackupConfigurationService> _logger = logger;

    public bool IsSupported => _automatedBackupScheduler.IsSupported;

    public bool GetIsEnabled()
    {
        return AutomatedBackupSettingsStore.GetIsEnabled(_appStoragePathService.DatabasePath);
    }

    public bool GetIsCloudEnabled()
    {
        return AutomatedBackupSettingsStore.GetIsCloudEnabled(_appStoragePathService.DatabasePath);
    }

    public bool GetHasAnyEnabled()
    {
        return AutomatedBackupSettingsStore.GetHasAnyEnabled(_appStoragePathService.DatabasePath);
    }

    public void SetIsEnabled(bool isEnabled)
    {
        var databasePath = _appStoragePathService.DatabasePath;
        var previousValue = AutomatedBackupSettingsStore.GetIsEnabled(databasePath);
        var previousHasAnyEnabled = AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath);

        _logger.LogInformation(
            "Updating automated local backup enabled state from {PreviousEnabled} to {NextEnabled}. Previous any-enabled state: {PreviousHasAnyEnabled}.",
            previousValue,
            isEnabled,
            previousHasAnyEnabled);

        AutomatedBackupSettingsStore.SetIsEnabled(databasePath, isEnabled);

        try
        {
            _automatedBackupScheduler.Synchronize(AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to update automated local backup enabled state to {NextEnabled}; restoring previous scheduler state. Previous enabled: {PreviousEnabled}. Previous any-enabled: {PreviousHasAnyEnabled}.",
                isEnabled,
                previousValue,
                previousHasAnyEnabled);
            AutomatedBackupSettingsStore.SetIsEnabled(databasePath, previousValue);
            _automatedBackupScheduler.Synchronize(previousHasAnyEnabled);
            throw;
        }

        _logger.LogInformation(
            "Automated local backup enabled state updated. Local enabled: {LocalEnabled}. Cloud enabled: {CloudEnabled}. Any enabled: {HasAnyEnabled}.",
            AutomatedBackupSettingsStore.GetIsEnabled(databasePath),
            AutomatedBackupSettingsStore.GetIsCloudEnabled(databasePath),
            AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath));
    }

    public void SetIsCloudEnabled(bool isEnabled)
    {
        var databasePath = _appStoragePathService.DatabasePath;
        var previousValue = AutomatedBackupSettingsStore.GetIsCloudEnabled(databasePath);
        var previousHasAnyEnabled = AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath);

        _logger.LogInformation(
            "Updating automated cloud backup enabled state from {PreviousEnabled} to {NextEnabled}. Previous any-enabled state: {PreviousHasAnyEnabled}.",
            previousValue,
            isEnabled,
            previousHasAnyEnabled);

        AutomatedBackupSettingsStore.SetIsCloudEnabled(databasePath, isEnabled);

        try
        {
            _automatedBackupScheduler.Synchronize(AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to update automated cloud backup enabled state to {NextEnabled}; restoring previous scheduler state. Previous enabled: {PreviousEnabled}. Previous any-enabled: {PreviousHasAnyEnabled}.",
                isEnabled,
                previousValue,
                previousHasAnyEnabled);
            AutomatedBackupSettingsStore.SetIsCloudEnabled(databasePath, previousValue);
            _automatedBackupScheduler.Synchronize(previousHasAnyEnabled);
            throw;
        }

        _logger.LogInformation(
            "Automated cloud backup enabled state updated. Local enabled: {LocalEnabled}. Cloud enabled: {CloudEnabled}. Any enabled: {HasAnyEnabled}.",
            AutomatedBackupSettingsStore.GetIsEnabled(databasePath),
            AutomatedBackupSettingsStore.GetIsCloudEnabled(databasePath),
            AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath));
    }

    public void SynchronizeScheduler()
    {
        var localEnabled = GetIsEnabled();
        var cloudEnabled = GetIsCloudEnabled();
        var hasAnyEnabled = localEnabled || cloudEnabled;

        _logger.LogInformation(
            "Synchronizing automated backup scheduler. Local enabled: {LocalEnabled}. Cloud enabled: {CloudEnabled}. Any enabled: {HasAnyEnabled}.",
            localEnabled,
            cloudEnabled,
            hasAnyEnabled);

        _automatedBackupScheduler.Synchronize(hasAnyEnabled);
    }
}
