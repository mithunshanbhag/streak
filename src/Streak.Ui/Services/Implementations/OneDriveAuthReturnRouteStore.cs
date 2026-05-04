namespace Streak.Ui.Services.Implementations;

public sealed class OneDriveAuthReturnRouteStore(
    ILogger<OneDriveAuthReturnRouteStore> logger)
    : IOneDriveAuthReturnRouteStore
{
    private const string PendingReturnRouteKey = "onedrive-auth.pending-return-route";
    private readonly ILogger<OneDriveAuthReturnRouteStore> _logger = logger;

    public void SetPendingReturnRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            _logger.LogWarning("Ignoring blank OneDrive post-auth return route.");
            return;
        }

        _logger.LogInformation("Persisting OneDrive post-auth return route {ReturnRoute}.", route);
        Preferences.Default.Set(PendingReturnRouteKey, route);
    }

    public string? ConsumePendingReturnRoute()
    {
        var route = Preferences.Default.Get<string?>(PendingReturnRouteKey, null);
        Preferences.Default.Remove(PendingReturnRouteKey);
        if (string.IsNullOrWhiteSpace(route))
        {
            _logger.LogDebug("No OneDrive post-auth return route was available to consume.");
            return null;
        }

        _logger.LogInformation("Consuming OneDrive post-auth return route {ReturnRoute}.", route);
        return route;
    }

    public void ClearPendingReturnRoute()
    {
        _logger.LogInformation("Clearing OneDrive post-auth return route.");
        Preferences.Default.Remove(PendingReturnRouteKey);
    }
}
