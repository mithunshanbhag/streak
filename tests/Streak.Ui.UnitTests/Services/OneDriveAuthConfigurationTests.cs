namespace Streak.Ui.UnitTests.Services;

public sealed class OneDriveAuthConfigurationTests
{
    #region Positive tests

    [Fact]
    public void OneDriveAuthConfiguration_ShouldBuildExpectedRedirectUri_ForConfiguredClientId()
    {
        var configuration = new OneDriveAuthConfiguration
        {
            ClientId = "11111111-2222-3333-4444-555555555555",
            Scopes = OneDriveAuthConstants.DefaultScopes
        };

        configuration.IsConfigured.Should().BeTrue();
        configuration.RedirectUri.Should().Be("msal11111111-2222-3333-4444-555555555555://auth");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void OneDriveAuthConfiguration_ShouldTreatPlaceholderClientIdAsNotConfigured()
    {
        var configuration = new OneDriveAuthConfiguration
        {
            ClientId = OneDriveAuthConstants.UnconfiguredClientId,
            Scopes = OneDriveAuthConstants.DefaultScopes
        };

        configuration.IsConfigured.Should().BeFalse();
    }

    #endregion
}
