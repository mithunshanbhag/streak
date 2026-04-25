using System.Diagnostics;
using Microsoft.Identity.Client;

namespace Streak.Ui.Platforms.Android.Services;

public sealed class AndroidOneDriveAuthService(
    IOneDriveAuthConfigurationProvider configurationProvider,
    IOneDriveAuthStateStore authStateStore,
    ILogger<AndroidOneDriveAuthService> logger)
    : IOneDriveAuthService
{
    private readonly IOneDriveAuthConfigurationProvider _configurationProvider = configurationProvider;
    private readonly IOneDriveAuthStateStore _authStateStore = authStateStore;
    private readonly ILogger<AndroidOneDriveAuthService> _logger = logger;

    private readonly SemaphoreSlim _publicClientInitializationLock = new(1, 1);

    private IPublicClientApplication? _publicClientApplication;

    public event EventHandler<OneDriveAuthState>? AuthStateChanged;

    public async Task<OneDriveConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var configuration = _configurationProvider.GetConfiguration();
        if (!configuration.IsConfigured)
        {
            _logger.LogWarning("OneDrive sign-in skipped because the build is not configured.");
            throw new InvalidOperationException("OneDrive sign-in is not configured for this build.");
        }

        _logger.LogInformation(
            "OneDrive interactive sign-in starting. Scope count: {ScopeCount}. Redirect host: {RedirectHost}. Redirect scheme length: {RedirectSchemeLength}.",
            configuration.Scopes.Count,
            OneDriveAuthConstants.RedirectUriHost,
            configuration.RedirectScheme.Length);

        var publicClientApplication = await GetPublicClientApplicationAsync(cancellationToken);
        var currentActivity = AndroidActivityTracker.GetRequiredCurrentActivity();

        try
        {
            _logger.LogInformation(
                "OneDrive interactive sign-in launching browser flow. Parent activity type: {ParentActivityType}. Current activity finishing: {CurrentActivityFinishing}.",
                currentActivity.GetType().Name,
                currentActivity.IsFinishing);

            var authenticationResult = await publicClientApplication
                .AcquireTokenInteractive(configuration.Scopes)
                .WithParentActivityOrWindow(currentActivity)
                .WithUseEmbeddedWebView(false)
                .ExecuteAsync(cancellationToken);

            _logger.LogInformation(
                "OneDrive interactive sign-in completed in {ElapsedMilliseconds} ms. Account returned: {AccountReturned}.",
                stopwatch.ElapsedMilliseconds,
                authenticationResult.Account is not null);

            _authStateStore.SetLastKnownAccountUsername(authenticationResult.Account?.Username);
            var authState = await GetAuthStateAsync(cancellationToken);
            NotifyAuthStateChanged(authState);

            return OneDriveConnectResult.Connected(authState);
        }
        catch (MsalException exception) when (IsUserCancellation(exception))
        {
            _logger.LogInformation(
                exception,
                "OneDrive sign-in was cancelled by the user after {ElapsedMilliseconds} ms. MSAL error code: {MsalErrorCode}.",
                stopwatch.ElapsedMilliseconds,
                exception.ErrorCode);

            return OneDriveConnectResult.Cancelled(await GetAuthStateAsync(cancellationToken));
        }
        catch (MsalException exception)
        {
            _logger.LogError(
                exception,
                "OneDrive sign-in failed after {ElapsedMilliseconds} ms. MSAL error code: {MsalErrorCode}.",
                stopwatch.ElapsedMilliseconds,
                exception.ErrorCode);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "OneDrive sign-in failed after {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var configuration = _configurationProvider.GetConfiguration();
        if (!configuration.IsConfigured)
        {
            _logger.LogInformation("OneDrive disconnect skipped because the build is not configured.");
            _authStateStore.Clear();
            NotifyAuthStateChanged(CreateAuthState(configuration, accountUsername: null));
            return;
        }

        try
        {
            _logger.LogInformation("OneDrive disconnect starting.");

            var publicClientApplication = await GetPublicClientApplicationAsync(cancellationToken);
            var accounts = await GetAccountsSnapshotAsync(publicClientApplication);
            var removedAccountCount = 0;

            foreach (var account in accounts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await publicClientApplication.RemoveAsync(account);
                removedAccountCount++;
            }

            _authStateStore.Clear();
            NotifyAuthStateChanged(CreateAuthState(configuration, accountUsername: null));

            _logger.LogInformation(
                "OneDrive disconnect completed in {ElapsedMilliseconds} ms. Account count: {AccountCount}. Removed account count: {RemovedAccountCount}.",
                stopwatch.ElapsedMilliseconds,
                accounts.Count,
                removedAccountCount);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "OneDrive disconnect failed after {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<OneDriveAuthState> GetAuthStateAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var configuration = _configurationProvider.GetConfiguration();
        if (!configuration.IsConfigured)
        {
            _logger.LogInformation("OneDrive auth state loaded as disconnected because the build is not configured.");
            _authStateStore.Clear();
            return CreateAuthState(configuration, accountUsername: null);
        }

        try
        {
            _logger.LogDebug("OneDrive auth state load starting.");

            var publicClientApplication = await GetPublicClientApplicationAsync(cancellationToken);
            _logger.LogDebug("OneDrive auth state account snapshot starting.");
            var accounts = await GetAccountsSnapshotAsync(publicClientApplication);
            var accountUsername = accounts.FirstOrDefault()?.Username;
            var cachedAccountUsername = _authStateStore.GetLastKnownAccountUsername();
            if (!string.IsNullOrWhiteSpace(accountUsername))
                _authStateStore.SetLastKnownAccountUsername(accountUsername);
            else
                accountUsername = cachedAccountUsername;

            _logger.LogInformation(
                "OneDrive auth state loaded in {ElapsedMilliseconds} ms. Account count: {AccountCount}. Cached account fallback present: {CachedAccountFallbackPresent}. Connected: {OneDriveConnected}.",
                stopwatch.ElapsedMilliseconds,
                accounts.Count,
                !string.IsNullOrWhiteSpace(cachedAccountUsername),
                !string.IsNullOrWhiteSpace(accountUsername));

            return CreateAuthState(configuration, accountUsername);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Unable to load persisted OneDrive auth state after {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);
            return CreateAuthState(configuration, _authStateStore.GetLastKnownAccountUsername());
        }
    }

    #region Private Helper Methods

    private static OneDriveAuthState CreateAuthState(OneDriveAuthConfiguration configuration, string? accountUsername)
    {
        return new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = configuration.IsConfigured,
            IsConnected = !string.IsNullOrWhiteSpace(accountUsername),
            AccountUsername = accountUsername
        };
    }

    private async Task<IPublicClientApplication> GetPublicClientApplicationAsync(CancellationToken cancellationToken)
    {
        if (_publicClientApplication is not null)
            return _publicClientApplication;

        await _publicClientInitializationLock.WaitAsync(cancellationToken);

        try
        {
            if (_publicClientApplication is not null)
                return _publicClientApplication;

            var configuration = _configurationProvider.GetConfiguration();
            if (!configuration.IsConfigured)
            {
                _logger.LogWarning("OneDrive public client initialization failed because the build is not configured.");
                throw new InvalidOperationException("OneDrive sign-in is not configured for this build.");
            }

            _logger.LogInformation(
                "OneDrive public client initialization starting. Authority: {Authority}. Redirect host: {RedirectHost}. Redirect scheme length: {RedirectSchemeLength}.",
                OneDriveAuthConstants.Authority,
                OneDriveAuthConstants.RedirectUriHost,
                configuration.RedirectScheme.Length);

            var publicClientApplication = PublicClientApplicationBuilder
                .Create(configuration.ClientId)
                .WithAuthority(OneDriveAuthConstants.Authority)
                .WithRedirectUri(configuration.RedirectUri)
                .Build();

            _publicClientApplication = publicClientApplication;
            _logger.LogInformation("OneDrive public client initialization completed.");

            return _publicClientApplication;
        }
        finally
        {
            _publicClientInitializationLock.Release();
        }
    }

    private static async Task<IReadOnlyList<IAccount>> GetAccountsSnapshotAsync(IPublicClientApplication publicClientApplication)
    {
        var accounts = await publicClientApplication.GetAccountsAsync();
        return accounts.ToArray();
    }

    private static bool IsUserCancellation(MsalException exception)
    {
        return string.Equals(exception.ErrorCode, MsalError.AuthenticationCanceledError, StringComparison.Ordinal)
               || string.Equals(exception.ErrorCode, "access_denied", StringComparison.OrdinalIgnoreCase);
    }

    private void NotifyAuthStateChanged(OneDriveAuthState authState)
    {
        AuthStateChanged?.Invoke(this, authState);
    }

    #endregion
}
