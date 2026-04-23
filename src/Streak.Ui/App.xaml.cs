namespace Streak.Ui;

public partial class App : Application
{
    private readonly IAppInitializationService _appInitializationService;
    private readonly ILogger<App> _logger;
    private readonly IReminderNotificationPermissionCoordinator _reminderNotificationPermissionCoordinator;

    public App(
        IAppInitializationService appInitializationService,
        ILogger<App> logger,
        IReminderNotificationPermissionCoordinator reminderNotificationPermissionCoordinator)
    {
        _appInitializationService = appInitializationService;
        _logger = logger;
        _reminderNotificationPermissionCoordinator = reminderNotificationPermissionCoordinator;

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage()) { Title = "Streak.Ui" };
        window.Created += OnWindowCreated;

        return window;
    }

    private async void OnWindowCreated(object? sender, EventArgs e)
    {
        try
        {
            await _appInitializationService.EnsureInitializedAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to finish app startup initialization.");
            return;
        }

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
