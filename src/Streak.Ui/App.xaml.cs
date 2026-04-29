namespace Streak.Ui;

public partial class App : Application
{
    private readonly IAppInitializationService _appInitializationService;
    private readonly ILogger<App> _logger;

    public App(
        IAppInitializationService appInitializationService,
        ILogger<App> logger)
    {
        StartupTiming.Mark("app-constructor-start");

        _appInitializationService = appInitializationService;
        _logger = logger;

        InitializeComponent();

        StartupTiming.Mark("app-constructor-completed");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        StartupTiming.Mark("app-create-window-start");

        var window = new Window(new MainPage()) { Title = "Streak.Ui" };
        window.Created += OnWindowCreated;

        StartupTiming.Mark("app-create-window-completed");

        return window;
    }

    private async void OnWindowCreated(object? sender, EventArgs e)
    {
        StartupTiming.Mark("app-window-created-handler-start");

        try
        {
            await _appInitializationService.EnsureInitializedAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to finish app startup initialization.");
            return;
        }

        StartupTiming.Mark("app-window-created-handler-completed");
    }
}
