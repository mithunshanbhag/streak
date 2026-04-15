namespace Streak.Ui.Services.Implementations;

public sealed class AutomatedBackupConfigurationService(
    IAppStoragePathService appStoragePathService,
    IAutomatedBackupScheduler automatedBackupScheduler)
    : IAutomatedBackupConfigurationService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IAutomatedBackupScheduler _automatedBackupScheduler = automatedBackupScheduler;

    public bool GetIsEnabled()
    {
        return AutomatedBackupSettingsStore.GetIsEnabled(_appStoragePathService.DatabasePath);
    }

    public void SetIsEnabled(bool isEnabled)
    {
        var databasePath = _appStoragePathService.DatabasePath;
        var previousValue = AutomatedBackupSettingsStore.GetIsEnabled(databasePath);

        AutomatedBackupSettingsStore.SetIsEnabled(databasePath, isEnabled);

        try
        {
            _automatedBackupScheduler.Synchronize(isEnabled);
        }
        catch
        {
            AutomatedBackupSettingsStore.SetIsEnabled(databasePath, previousValue);
            _automatedBackupScheduler.Synchronize(previousValue);
            throw;
        }
    }

    public void SynchronizeScheduler()
    {
        _automatedBackupScheduler.Synchronize(GetIsEnabled());
    }
}
