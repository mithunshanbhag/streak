using Microsoft.Extensions.Configuration;

namespace Streak.Ui.Misc.Utilities;

public static class ApplicationInsightsConfigurationUtility
{
    public static void AddAppSettings(IConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        using var appSettingsStream = FileSystem.OpenAppPackageFileAsync(AppConstants.AppSettingsFileName)
            .GetAwaiter()
            .GetResult();

        configurationBuilder.AddJsonStream(appSettingsStream);
    }

    public static ApplicationInsightsConfig GetApplicationInsightsConfig(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var applicationInsightsConfig = new ApplicationInsightsConfig();
        configuration.GetSection(ConfigKeys.ApplicationInsightsSectionName).Bind(applicationInsightsConfig);
        applicationInsightsConfig.ConnectionString = applicationInsightsConfig.ConnectionString.Trim();

        return applicationInsightsConfig;
    }
}
