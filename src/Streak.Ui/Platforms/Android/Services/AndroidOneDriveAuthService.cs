using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Streak.Ui.Platforms.Android.Services;

public sealed class AndroidOneDriveAuthService(
    IOneDriveAuthConfigurationProvider configurationProvider,
    ILogger<AndroidOneDriveAuthService> logger)
    : IOneDriveAuthService
{
    private readonly IOneDriveAuthConfigurationProvider _configurationProvider = configurationProvider;
    private readonly ILogger<AndroidOneDriveAuthService> _logger = logger;

    private readonly SemaphoreSlim _publicClientInitializationLock = new(1, 1);

    private IPublicClientApplication? _publicClientApplication;

    public async Task<OneDriveConnectResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var configuration = _configurationProvider.GetConfiguration();
        if (!configuration.IsConfigured)
            throw new InvalidOperationException("OneDrive sign-in is not configured for this build.");

        var publicClientApplication = await GetPublicClientApplicationAsync(cancellationToken);
        var currentActivity = AndroidActivityTracker.GetRequiredCurrentActivity();

        try
        {
            var authenticationResult = await publicClientApplication
                .AcquireTokenInteractive(configuration.Scopes)
                .WithParentActivityOrWindow(currentActivity)
                .WithUseEmbeddedWebView(false)
                .ExecuteAsync(cancellationToken);

            return OneDriveConnectResult.Connected(
                CreateAuthState(
                    configuration,
                    authenticationResult.Account?.Username));
        }
        catch (MsalException exception) when (IsUserCancellation(exception))
        {
            _logger.LogInformation("OneDrive sign-in was cancelled by the user.");

            return OneDriveConnectResult.Cancelled(await GetAuthStateAsync(cancellationToken));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "OneDrive sign-in failed.");
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        var configuration = _configurationProvider.GetConfiguration();
        if (!configuration.IsConfigured)
            return;

        try
        {
            var publicClientApplication = await GetPublicClientApplicationAsync(cancellationToken);
            var accounts = await publicClientApplication.GetAccountsAsync();

            foreach (var account in accounts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await publicClientApplication.RemoveAsync(account);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "OneDrive disconnect failed.");
            throw;
        }
    }

    public async Task<OneDriveAuthState> GetAuthStateAsync(CancellationToken cancellationToken = default)
    {
        var configuration = _configurationProvider.GetConfiguration();
        if (!configuration.IsConfigured)
            return CreateAuthState(configuration, accountUsername: null);

        try
        {
            var publicClientApplication = await GetPublicClientApplicationAsync(cancellationToken);
            var account = await GetPrimaryAccountAsync(publicClientApplication);

            return CreateAuthState(configuration, account?.Username);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Unable to load persisted OneDrive auth state.");
            return CreateAuthState(configuration, accountUsername: null);
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
                throw new InvalidOperationException("OneDrive sign-in is not configured for this build.");

            var publicClientApplication = PublicClientApplicationBuilder
                .Create(configuration.ClientId)
                .WithAuthority(OneDriveAuthConstants.Authority)
                .WithRedirectUri(configuration.RedirectUri)
                .Build();

            var storageProperties = new StorageCreationPropertiesBuilder(
                    OneDriveAuthConstants.TokenCacheFileName,
                    FileSystem.AppDataDirectory)
                .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
            cacheHelper.RegisterCache(publicClientApplication.UserTokenCache);

            _publicClientApplication = publicClientApplication;
            return _publicClientApplication;
        }
        finally
        {
            _publicClientInitializationLock.Release();
        }
    }

    private static async Task<IAccount?> GetPrimaryAccountAsync(IPublicClientApplication publicClientApplication)
    {
        var accounts = await publicClientApplication.GetAccountsAsync();
        return accounts.FirstOrDefault();
    }

    private static bool IsUserCancellation(MsalException exception)
    {
        return string.Equals(exception.ErrorCode, MsalError.AuthenticationCanceledError, StringComparison.Ordinal)
               || string.Equals(exception.ErrorCode, "access_denied", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
