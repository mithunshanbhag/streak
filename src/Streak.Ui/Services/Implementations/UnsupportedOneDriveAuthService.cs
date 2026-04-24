namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedOneDriveAuthService : IOneDriveAuthService
{
    public Task<OneDriveConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("OneDrive sign-in is not supported on this platform.");
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<OneDriveAuthState> GetAuthStateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new OneDriveAuthState
        {
            IsPlatformSupported = false,
            IsConfigured = false,
            IsConnected = false
        });
    }
}
