namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupConfigurationService
{
    /// <summary>
    ///     Gets whether nightly automated backups are supported on the current platform.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    ///     Gets whether nightly automated backups are currently enabled in persisted local settings.
    /// </summary>
    /// <returns><see langword="true" /> when nightly automated backups are enabled; otherwise, <see langword="false" />.</returns>
    bool GetIsEnabled();

    /// <summary>
    ///     Persists the nightly automated backup enabled state and synchronizes the operating system registration to match it.
    /// </summary>
    /// <param name="isEnabled">
    ///     <see langword="true" /> to enable nightly automated backups; <see langword="false" /> to disable them.
    /// </param>
    void SetIsEnabled(bool isEnabled);

    /// <summary>
    ///     Reads the persisted nightly automated backup enabled state and synchronizes operating system registration to match it.
    /// </summary>
    void SynchronizeScheduler();
}
