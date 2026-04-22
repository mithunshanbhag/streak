namespace Streak.Ui.Services.Implementations;

public sealed class ReminderNotificationPermissionCoordinator(
    IReminderConfigurationService reminderConfigurationService,
    IReminderNotificationPermissionService reminderNotificationPermissionService)
    : IReminderNotificationPermissionCoordinator
{
    private readonly IReminderConfigurationService _reminderConfigurationService = reminderConfigurationService;
    private readonly IReminderNotificationPermissionService _reminderNotificationPermissionService = reminderNotificationPermissionService;

    public Task<bool> RequestPermissionIfRemindersEnabledAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return !_reminderConfigurationService.GetIsEnabled()
            ? Task.FromResult(true)
            : _reminderNotificationPermissionService.RequestPermissionIfNeededAsync(cancellationToken);
    }
}
