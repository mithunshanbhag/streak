namespace Streak.Ui;

public partial class App : Application
{
    private readonly IAutomatedBackupConfigurationService _automatedBackupConfigurationService;
    private readonly ILogger<App> _logger;
    private readonly IReminderConfigurationService _reminderConfigurationService;
    private readonly IReminderNotificationPermissionCoordinator _reminderNotificationPermissionCoordinator;
    private readonly SqliteDatabaseBootstrapper _sqliteDatabaseBootstrapper;
    private readonly SqliteDatabaseSchemaUpgrader _sqliteDatabaseSchemaUpgrader;

    public App(
        IAutomatedBackupConfigurationService automatedBackupConfigurationService,
        ILogger<App> logger,
        IReminderConfigurationService reminderConfigurationService,
        IReminderNotificationPermissionCoordinator reminderNotificationPermissionCoordinator,
        SqliteDatabaseBootstrapper sqliteDatabaseBootstrapper,
        SqliteDatabaseSchemaUpgrader sqliteDatabaseSchemaUpgrader)
    {
        _automatedBackupConfigurationService = automatedBackupConfigurationService;
        _logger = logger;
        _reminderConfigurationService = reminderConfigurationService;
        _reminderNotificationPermissionCoordinator = reminderNotificationPermissionCoordinator;
        _sqliteDatabaseBootstrapper = sqliteDatabaseBootstrapper;
        _sqliteDatabaseSchemaUpgrader = sqliteDatabaseSchemaUpgrader;

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _sqliteDatabaseBootstrapper.EnsureDbExists();
        _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(SqliteDatabaseBootstrapper.DatabasePath);
        _automatedBackupConfigurationService.SynchronizeScheduler();
        _reminderConfigurationService.SynchronizeScheduler();

        var window = new Window(new MainPage()) { Title = "Streak.Ui" };
        window.Created += OnWindowCreated;

        return window;
    }

    private async void OnWindowCreated(object? sender, EventArgs e)
    {
        try
        {
            await _reminderNotificationPermissionCoordinator.RequestPermissionIfRemindersEnabledAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Unable to request reminder notification permission on app startup.");
        }
    }
}
