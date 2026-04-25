namespace Streak.Ui.Services.Models;

public readonly record struct OneDriveConnectResult(OneDriveConnectStatus Status, OneDriveAuthState AuthState)
{
    public static OneDriveConnectResult Cancelled(OneDriveAuthState authState)
    {
        return new OneDriveConnectResult(OneDriveConnectStatus.Cancelled, authState);
    }

    public static OneDriveConnectResult Connected(OneDriveAuthState authState)
    {
        return new OneDriveConnectResult(OneDriveConnectStatus.Connected, authState);
    }
}
