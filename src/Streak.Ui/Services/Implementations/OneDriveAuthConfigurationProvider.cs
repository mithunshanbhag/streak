namespace Streak.Ui.Services.Implementations;

public sealed class OneDriveAuthConfigurationProvider : IOneDriveAuthConfigurationProvider
{
    private readonly Lazy<OneDriveAuthConfiguration> _configuration = new(CreateConfiguration);

    public OneDriveAuthConfiguration GetConfiguration()
    {
        return _configuration.Value;
    }

    #region Private Helper Methods

    private static OneDriveAuthConfiguration CreateConfiguration()
    {
        var clientId = typeof(App).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => string.Equals(
                attribute.Key,
                OneDriveAuthConstants.AssemblyMetadataClientIdKey,
                StringComparison.Ordinal))
            ?.Value;

        clientId = string.IsNullOrWhiteSpace(clientId)
            ? OneDriveAuthConstants.UnconfiguredClientId
            : clientId.Trim();

        return new OneDriveAuthConfiguration
        {
            ClientId = clientId,
            Scopes = OneDriveAuthConstants.DefaultScopes
        };
    }

    #endregion
}
