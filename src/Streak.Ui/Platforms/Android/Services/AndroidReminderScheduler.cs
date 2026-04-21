using Android.App;
using Application = Android.App.Application;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidReminderScheduler(
    TimeProvider timeProvider,
    ILogger<AndroidReminderScheduler> logger)
    : IReminderScheduler
{
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<AndroidReminderScheduler> _logger = logger;

    public void Synchronize(bool isEnabled, TimeOnly timeLocal)
    {
        var nextRunUtc = AndroidReminderAlarmRegistrar.Synchronize(
            Application.Context,
            _timeProvider,
            isEnabled,
            timeLocal);

        if (nextRunUtc is null)
        {
            _logger.LogInformation("Cancelled daily reminder trigger.");
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, _timeProvider.LocalTimeZone);
        _logger.LogInformation("Scheduled daily reminder trigger for {NextRunLocal}.", nextRunLocal);
    }
}
