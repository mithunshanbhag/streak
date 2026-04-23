namespace Streak.Ui.Services.Implementations;

public sealed class AppInitializationService(
    IAppStartupWorkService appStartupWorkService,
    ILogger<AppInitializationService> logger)
    : IAppInitializationService
{
    private readonly IAppStartupWorkService _appStartupWorkService = appStartupWorkService;
    private readonly ILogger<AppInitializationService> _logger = logger;

    private readonly object _initializationSync = new();

    private Task? _initializationTask;

    public Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var initializationTask = GetOrCreateInitializationTask();

        return cancellationToken.CanBeCanceled
            ? initializationTask.WaitAsync(cancellationToken)
            : initializationTask;
    }

    #region Private Helper Methods

    private Task GetOrCreateInitializationTask()
    {
        lock (_initializationSync)
        {
            return _initializationTask ??= InitializeCoreAsync();
        }
    }

    private async Task InitializeCoreAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Running app startup initialization.");

        try
        {
            await Task.Run(_appStartupWorkService.Execute);

            _logger.LogInformation(
                "App startup initialization completed in {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "App startup initialization failed after {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    #endregion
}
