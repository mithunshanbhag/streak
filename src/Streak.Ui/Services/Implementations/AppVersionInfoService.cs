namespace Streak.Ui.Services.Implementations;

public sealed class AppVersionInfoService(IAppInfo appInfo) : IAppVersionInfoService
{
    public AppVersionInfo GetCurrent()
    {
        return new AppVersionInfo
        {
            DisplayVersion = GetRequiredMetadataValue(appInfo.VersionString, nameof(IAppInfo.VersionString)),
            BuildNumber = GetRequiredMetadataValue(appInfo.BuildString, nameof(IAppInfo.BuildString))
        };
    }

    private static string GetRequiredMetadataValue(string? value, string metadataName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"App metadata value '{metadataName}' is missing.");

        return value;
    }
}
