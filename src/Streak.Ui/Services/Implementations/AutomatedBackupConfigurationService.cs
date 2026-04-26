namespace Streak.Ui.Services.Implementations;

public sealed class AutomatedBackupConfigurationService(
    IAppStoragePathService appStoragePathService,
    IAutomatedBackupScheduler automatedBackupScheduler)
    : IAutomatedBackupConfigurationService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IAutomatedBackupScheduler _automatedBackupScheduler = automatedBackupScheduler;

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

        AutomatedBackupSettingsStore.SetIsEnabled(databasePath, isEnabled);

        try
        {
            _automatedBackupScheduler.Synchronize(AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath));
        }
        catch
        {
            AutomatedBackupSettingsStore.SetIsEnabled(databasePath, previousValue);
            _automatedBackupScheduler.Synchronize(previousHasAnyEnabled);
            throw;
        }
    }

    public void SetIsCloudEnabled(bool isEnabled)
    {
        var databasePath = _appStoragePathService.DatabasePath;
        var previousValue = AutomatedBackupSettingsStore.GetIsCloudEnabled(databasePath);
        var previousHasAnyEnabled = AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath);

        AutomatedBackupSettingsStore.SetIsCloudEnabled(databasePath, isEnabled);

        try
        {
            _automatedBackupScheduler.Synchronize(AutomatedBackupSettingsStore.GetHasAnyEnabled(databasePath));
        }
        catch
        {
            AutomatedBackupSettingsStore.SetIsCloudEnabled(databasePath, previousValue);
            _automatedBackupScheduler.Synchronize(previousHasAnyEnabled);
            throw;
        }
    }

    public void SynchronizeScheduler()
    {
        _automatedBackupScheduler.Synchronize(GetHasAnyEnabled());
    }
}
