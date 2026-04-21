namespace Streak.Ui.Services.Implementations;

public sealed class ReminderConfigurationService(
    IAppStoragePathService appStoragePathService,
    IReminderScheduler reminderScheduler)
    : IReminderConfigurationService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IReminderScheduler _reminderScheduler = reminderScheduler;

    public bool GetIsEnabled()
    {
        return ReminderSettingsStore.GetSettings(_appStoragePathService.DatabasePath).IsEnabled;
    }

    public TimeOnly GetTimeLocal()
    {
        return ReminderSettingsStore.GetSettings(_appStoragePathService.DatabasePath).TimeLocal;
    }

    public void SetIsEnabled(bool isEnabled)
    {
        var databasePath = _appStoragePathService.DatabasePath;
        var previousSettings = ReminderSettingsStore.GetSettings(databasePath);
        var nextSettings = previousSettings with { IsEnabled = isEnabled };

        ReminderSettingsStore.SetSettings(databasePath, nextSettings);

        try
        {
            _reminderScheduler.Synchronize(nextSettings.IsEnabled, nextSettings.TimeLocal);
        }
        catch
        {
            ReminderSettingsStore.SetSettings(databasePath, previousSettings);
            _reminderScheduler.Synchronize(previousSettings.IsEnabled, previousSettings.TimeLocal);
            throw;
        }
    }

    public void SetTimeLocal(TimeOnly timeLocal)
    {
        var databasePath = _appStoragePathService.DatabasePath;
        var previousSettings = ReminderSettingsStore.GetSettings(databasePath);
        var nextSettings = previousSettings with { TimeLocal = timeLocal };

        ReminderSettingsStore.SetSettings(databasePath, nextSettings);

        try
        {
            _reminderScheduler.Synchronize(nextSettings.IsEnabled, nextSettings.TimeLocal);
        }
        catch
        {
            ReminderSettingsStore.SetSettings(databasePath, previousSettings);
            _reminderScheduler.Synchronize(previousSettings.IsEnabled, previousSettings.TimeLocal);
            throw;
        }
    }

    public void SynchronizeScheduler()
    {
        var settings = ReminderSettingsStore.GetSettings(_appStoragePathService.DatabasePath);
        _reminderScheduler.Synchronize(settings.IsEnabled, settings.TimeLocal);
    }
}
