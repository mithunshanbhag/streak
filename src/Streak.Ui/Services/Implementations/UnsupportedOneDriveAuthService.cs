namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedOneDriveAuthService : IOneDriveAuthService
{
    public event EventHandler<OneDriveAuthState>? AuthStateChanged;

    public Task<OneDriveConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("OneDrive sign-in is not supported on this platform.");
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        AuthStateChanged?.Invoke(this, CreateUnsupportedAuthState());
        return Task.CompletedTask;
    }

    public Task<OneDriveAuthState> GetAuthStateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateUnsupportedAuthState());
    }

    private static OneDriveAuthState CreateUnsupportedAuthState()
    {
        return new OneDriveAuthState
        {
            IsPlatformSupported = false,
            IsConfigured = false,
            IsConnected = false
        };
    }
}
