namespace Streak.Ui.Services.Implementations;

public sealed class OneDriveAuthReturnRouteStore : IOneDriveAuthReturnRouteStore
{
    private const string PendingReturnRouteKey = "onedrive-auth.pending-return-route";

    public void SetPendingReturnRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return;

        Preferences.Default.Set(PendingReturnRouteKey, route);
    }

    public string? ConsumePendingReturnRoute()
    {
        var route = Preferences.Default.Get<string?>(PendingReturnRouteKey, null);
        Preferences.Default.Remove(PendingReturnRouteKey);
        return string.IsNullOrWhiteSpace(route)
            ? null
            : route;
    }

    public void ClearPendingReturnRoute()
    {
        Preferences.Default.Remove(PendingReturnRouteKey);
    }
}
