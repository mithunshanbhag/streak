namespace Streak.Ui.UnitTests.Services;

public sealed class StreakAuthenticationStateProviderTests
{
    #region Positive tests

    [Fact]
    public async Task StreakAuthenticationStateProvider_ShouldReturnAuthenticatedUser_WhenOneDriveIsConnected()
    {
        var authService = new TestOneDriveAuthService(new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = true,
            IsConnected = true,
            AccountUsername = "streak-demo@outlook.com"
        });
        var provider = new StreakAuthenticationStateProvider(authService);

        var authState = await provider.GetAuthenticationStateAsync();

        authState.User.Identity?.IsAuthenticated.Should().BeTrue();
        authState.User.Identity?.Name.Should().Be("streak-demo@outlook.com");
        authState.User.Claims.Should().Contain(claim => claim.Type == "streak:auth_provider" && claim.Value == "OneDrive");
    }

    [Fact]
    public async Task StreakAuthenticationStateProvider_ShouldNotifyConsumers_WhenOneDriveAuthStateChanges()
    {
        var authService = new TestOneDriveAuthService(CreateDisconnectedState());
        var provider = new StreakAuthenticationStateProvider(authService);
        var notification = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.AuthenticationStateChanged += async task => notification.TrySetResult(await task);

        authService.SetAuthState(new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = true,
            IsConnected = true,
            AccountUsername = "streak-demo@outlook.com"
        });

        var authState = await notification.Task;
        authState.User.Identity?.IsAuthenticated.Should().BeTrue();
        authState.User.Identity?.Name.Should().Be("streak-demo@outlook.com");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public async Task StreakAuthenticationStateProvider_ShouldReturnAnonymousUser_WhenOneDriveIsDisconnected()
    {
        var provider = new StreakAuthenticationStateProvider(new TestOneDriveAuthService(CreateDisconnectedState()));

        var authState = await provider.GetAuthenticationStateAsync();

        authState.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task StreakAuthenticationStateProvider_ShouldReturnAnonymousUser_WhenOneDriveIsUnsupported()
    {
        var provider = new StreakAuthenticationStateProvider(new TestOneDriveAuthService(new OneDriveAuthState
        {
            IsPlatformSupported = false,
            IsConfigured = false,
            IsConnected = false
        }));

        var authState = await provider.GetAuthenticationStateAsync();

        authState.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task StreakAuthenticationStateProvider_ShouldReturnAnonymousUser_WhenOneDriveIsUnconfigured()
    {
        var provider = new StreakAuthenticationStateProvider(new TestOneDriveAuthService(new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = false,
            IsConnected = false
        }));

        var authState = await provider.GetAuthenticationStateAsync();

        authState.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    #endregion

    #region Private Helper Methods

    private static OneDriveAuthState CreateDisconnectedState()
    {
        return new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = true,
            IsConnected = false
        };
    }

    private sealed class TestOneDriveAuthService(OneDriveAuthState authState) : IOneDriveAuthService
    {
        private OneDriveAuthState _authState = authState;

        public event EventHandler<OneDriveAuthState>? AuthStateChanged;

        public Task<OneDriveConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OneDriveConnectResult.Connected(_authState));
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            _authState = CreateDisconnectedState();
            AuthStateChanged?.Invoke(this, _authState);
            return Task.CompletedTask;
        }

        public Task<OneDriveAuthState> GetAuthStateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_authState);
        }

        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult("test-access-token");
        }

        public void SetAuthState(OneDriveAuthState authState)
        {
            _authState = authState;
            AuthStateChanged?.Invoke(this, _authState);
        }
    }

    #endregion
}
