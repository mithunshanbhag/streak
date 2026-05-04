using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Streak.Ui.Misc.Utilities;

public static class ApplicationInsightsLoggingUtility
{
    public static void Configure(ILoggingBuilder loggingBuilder, IConfiguration configuration, bool isDebugBuild)
    {
        ArgumentNullException.ThrowIfNull(loggingBuilder);
        ArgumentNullException.ThrowIfNull(configuration);

        loggingBuilder.ClearProviders();
        loggingBuilder.Configure(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.TraceId
                | ActivityTrackingOptions.SpanId
                | ActivityTrackingOptions.ParentId;
        });
        loggingBuilder.AddConfiguration(configuration.GetSection(ConfigKeys.LoggingSectionName));

        if (isDebugBuild)
        {
            loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            loggingBuilder.AddDebug();
            return;
        }

        var applicationInsightsConfig = ApplicationInsightsConfigurationUtility.GetApplicationInsightsConfig(configuration);
        if (!applicationInsightsConfig.IsConfigured)
            return;

        loggingBuilder.AddApplicationInsights(
            configureTelemetryConfiguration: telemetryConfiguration =>
            {
                telemetryConfiguration.ConnectionString = applicationInsightsConfig.ConnectionString;
            },
            configureApplicationInsightsLoggerOptions: options =>
            {
                options.IncludeScopes = true;
                options.TrackExceptionsAsExceptionTelemetry = true;
            });

        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);
    }
}
