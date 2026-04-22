namespace Streak.Ui.Services.Interfaces;

public interface IAppVersionInfoService
{
    /// <summary>
    ///     Gets the current app display version and build number from runtime app metadata.
    /// </summary>
    AppVersionInfo GetCurrent();
}
