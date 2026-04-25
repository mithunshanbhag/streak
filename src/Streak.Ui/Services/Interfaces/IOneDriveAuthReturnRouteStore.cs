namespace Streak.Ui.Services.Interfaces;

public interface IOneDriveAuthReturnRouteStore
{
    void SetPendingReturnRoute(string route);

    string? ConsumePendingReturnRoute();

    void ClearPendingReturnRoute();
}
