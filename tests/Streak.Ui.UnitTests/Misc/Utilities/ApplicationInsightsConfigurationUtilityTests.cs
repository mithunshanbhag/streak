namespace Streak.Ui.UnitTests.Misc.Utilities;

public sealed class ApplicationInsightsConfigurationUtilityTests
{
    #region Positive tests

    [Fact]
    public void GetApplicationInsightsConfig_ShouldBindConnectionString_FromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConfigKeys.ApplicationInsightsConnectionString] = "InstrumentationKey=test-key;IngestionEndpoint=https://example.com/"
            })
            .Build();

        var result = ApplicationInsightsConfigurationUtility.GetApplicationInsightsConfig(configuration);

        result.IsConfigured.Should().BeTrue();
        result.ConnectionString.Should().Be("InstrumentationKey=test-key;IngestionEndpoint=https://example.com/");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void GetApplicationInsightsConfig_ShouldTrimConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConfigKeys.ApplicationInsightsConnectionString] = "  InstrumentationKey=test-key;IngestionEndpoint=https://example.com/  "
            })
            .Build();

        var result = ApplicationInsightsConfigurationUtility.GetApplicationInsightsConfig(configuration);

        result.ConnectionString.Should().Be("InstrumentationKey=test-key;IngestionEndpoint=https://example.com/");
    }

    [Fact]
    public void GetApplicationInsightsConfig_ShouldRemainUnconfigured_WhenConnectionStringIsMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var result = ApplicationInsightsConfigurationUtility.GetApplicationInsightsConfig(configuration);

        result.IsConfigured.Should().BeFalse();
        result.ConnectionString.Should().BeEmpty();
    }

    #endregion
}
