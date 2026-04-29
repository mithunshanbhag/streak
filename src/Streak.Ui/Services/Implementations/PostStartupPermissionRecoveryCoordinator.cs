namespace Streak.Ui.Services.Implementations;

public sealed class PostStartupPermissionRecoveryCoordinator(
    IReminderConfigurationService reminderConfigurationService,
    IAutomatedBackupConfigurationService automatedBackupConfigurationService,
    IReminderNotificationPermissionService reminderNotificationPermissionService,
    ICheckinProofService checkinProofService,
    ICameraPermissionService cameraPermissionService,
    ILogger<PostStartupPermissionRecoveryCoordinator> logger)
    : IPostStartupPermissionRecoveryCoordinator
{
    private readonly IReminderConfigurationService _reminderConfigurationService = reminderConfigurationService;
    private readonly IAutomatedBackupConfigurationService _automatedBackupConfigurationService = automatedBackupConfigurationService;
    private readonly IReminderNotificationPermissionService _reminderNotificationPermissionService = reminderNotificationPermissionService;
    private readonly ICheckinProofService _checkinProofService = checkinProofService;
    private readonly ICameraPermissionService _cameraPermissionService = cameraPermissionService;
    private readonly ILogger<PostStartupPermissionRecoveryCoordinator> _logger = logger;

    private readonly SemaphoreSlim _permissionRecoveryLock = new(1, 1);

    private bool _hasRecoveredPermissions;

    public async Task RecoverMissingPermissionsAfterHomepageRenderAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_hasRecoveredPermissions)
            return;

        await _permissionRecoveryLock.WaitAsync(cancellationToken);

        try
        {
            if (_hasRecoveredPermissions)
                return;

            var remindersEnabled = _reminderConfigurationService.GetIsEnabled();
            var automatedBackupsEnabled = _automatedBackupConfigurationService.GetHasAnyEnabled();

            if (remindersEnabled || automatedBackupsEnabled)
            {
                var notificationsEnabled = await _reminderNotificationPermissionService.RequestPermissionIfNeededAsync(cancellationToken);
                _logger.LogInformation(
                    "Post-startup notification permission recovery completed. Reminders enabled: {RemindersEnabled}. Automated backups enabled: {AutomatedBackupsEnabled}. Notifications enabled: {NotificationsEnabled}.",
                    remindersEnabled,
                    automatedBackupsEnabled,
                    notificationsEnabled);
            }

            if (_checkinProofService.SupportsCameraCapture)
            {
                var cameraEnabled = await _cameraPermissionService.RequestPermissionIfNeededAsync(cancellationToken);
                _logger.LogInformation(
                    "Post-startup camera permission recovery completed. Camera enabled: {CameraEnabled}.",
                    cameraEnabled);
            }

            _hasRecoveredPermissions = true;
        }
        finally
        {
            _permissionRecoveryLock.Release();
        }
    }
}
