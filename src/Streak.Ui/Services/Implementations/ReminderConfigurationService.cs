namespace Streak.Ui.Services.Implementations;

public sealed class ReminderConfigurationService(
    IAppStoragePathService appStoragePathService,
    IReminderScheduler reminderScheduler,
    ILogger<ReminderConfigurationService> logger)
    : IReminderConfigurationService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IReminderScheduler _reminderScheduler = reminderScheduler;
    private readonly ILogger<ReminderConfigurationService> _logger = logger;

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

        _logger.LogInformation(
            "Updating reminder enabled state from {PreviousEnabled} to {NextEnabled} at {TimeLocal}.",
            previousSettings.IsEnabled,
            nextSettings.IsEnabled,
            nextSettings.TimeLocal);

        ReminderSettingsStore.SetSettings(databasePath, nextSettings);

        try
        {
            _reminderScheduler.Synchronize(nextSettings.IsEnabled, nextSettings.TimeLocal);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to update reminder enabled state to {NextEnabled}; restoring previous reminder settings. Previous enabled: {PreviousEnabled}. Previous time: {PreviousTimeLocal}.",
                nextSettings.IsEnabled,
                previousSettings.IsEnabled,
                previousSettings.TimeLocal);
            ReminderSettingsStore.SetSettings(databasePath, previousSettings);
            _reminderScheduler.Synchronize(previousSettings.IsEnabled, previousSettings.TimeLocal);
            throw;
        }

        _logger.LogInformation(
            "Reminder enabled state updated. Enabled: {RemindersEnabled}. TimeLocal: {TimeLocal}.",
            nextSettings.IsEnabled,
            nextSettings.TimeLocal);
    }

    public void SetTimeLocal(TimeOnly timeLocal)
    {
        var databasePath = _appStoragePathService.DatabasePath;
        var previousSettings = ReminderSettingsStore.GetSettings(databasePath);
        var nextSettings = previousSettings with { TimeLocal = timeLocal };

        _logger.LogInformation(
            "Updating reminder time from {PreviousTimeLocal} to {NextTimeLocal}. Reminders enabled: {RemindersEnabled}.",
            previousSettings.TimeLocal,
            nextSettings.TimeLocal,
            nextSettings.IsEnabled);

        ReminderSettingsStore.SetSettings(databasePath, nextSettings);

        try
        {
            _reminderScheduler.Synchronize(nextSettings.IsEnabled, nextSettings.TimeLocal);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to update reminder time to {NextTimeLocal}; restoring previous reminder settings. Previous enabled: {PreviousEnabled}. Previous time: {PreviousTimeLocal}.",
                nextSettings.TimeLocal,
                previousSettings.IsEnabled,
                previousSettings.TimeLocal);
            ReminderSettingsStore.SetSettings(databasePath, previousSettings);
            _reminderScheduler.Synchronize(previousSettings.IsEnabled, previousSettings.TimeLocal);
            throw;
        }

        _logger.LogInformation(
            "Reminder time updated. Enabled: {RemindersEnabled}. TimeLocal: {TimeLocal}.",
            nextSettings.IsEnabled,
            nextSettings.TimeLocal);
    }

    public void SynchronizeScheduler()
    {
        var settings = ReminderSettingsStore.GetSettings(_appStoragePathService.DatabasePath);
        _logger.LogInformation(
            "Synchronizing reminder scheduler. Enabled: {RemindersEnabled}. TimeLocal: {TimeLocal}.",
            settings.IsEnabled,
            settings.TimeLocal);
        _reminderScheduler.Synchronize(settings.IsEnabled, settings.TimeLocal);
    }
}
