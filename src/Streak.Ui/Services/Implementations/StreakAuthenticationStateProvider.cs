namespace Streak.Ui.Services.Implementations;

public sealed class StreakAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private const string AuthenticationType = "Streak.OneDrive";

    private readonly IOneDriveAuthService _oneDriveAuthService;

    public StreakAuthenticationStateProvider(IOneDriveAuthService oneDriveAuthService)
    {
        _oneDriveAuthService = oneDriveAuthService;
        _oneDriveAuthService.AuthStateChanged += HandleAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var authState = await _oneDriveAuthService.GetAuthStateAsync();
        return CreateAuthenticationState(authState);
    }

    public void Dispose()
    {
        _oneDriveAuthService.AuthStateChanged -= HandleAuthStateChanged;
    }

    private static AuthenticationState CreateAuthenticationState(OneDriveAuthState authState)
    {
        if (!authState.IsPlatformSupported || !authState.IsConfigured || !authState.IsConnected)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var accountUsername = string.IsNullOrWhiteSpace(authState.AccountUsername)
            ? "Microsoft account"
            : authState.AccountUsername;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, accountUsername),
            new("streak:auth_provider", "OneDrive")
        };

        if (!string.IsNullOrWhiteSpace(authState.AccountUsername))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, authState.AccountUsername));

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationType)));
    }

    private void HandleAuthStateChanged(object? sender, OneDriveAuthState authState)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(authState)));
    }
}
