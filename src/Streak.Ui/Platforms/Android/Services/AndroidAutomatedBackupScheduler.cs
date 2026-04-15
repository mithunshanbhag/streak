using Android.App;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidAutomatedBackupScheduler(
    TimeProvider timeProvider,
    ILogger<AndroidAutomatedBackupScheduler> logger)
    : IAutomatedBackupScheduler
{
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<AndroidAutomatedBackupScheduler> _logger = logger;

    public bool IsSupported => true;

    public void Synchronize(bool isEnabled)
    {
        var nextRunUtc = AndroidAutomatedBackupAlarmRegistrar.Synchronize(
            Android.App.Application.Context,
            _timeProvider,
            isEnabled);

        if (nextRunUtc is null)
        {
            _logger.LogInformation("Cancelled nightly automated backup trigger.");
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, _timeProvider.LocalTimeZone);
        _logger.LogInformation("Scheduled nightly automated backup trigger for {NextRunLocal}.", nextRunLocal);
    }
}
