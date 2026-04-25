namespace Streak.Ui.Services.Interfaces;

public interface IOneDriveAuthConfigurationProvider
{
    /// <summary>
    ///     Gets the current OneDrive authentication configuration for the app build.
    /// </summary>
    OneDriveAuthConfiguration GetConfiguration();
}
