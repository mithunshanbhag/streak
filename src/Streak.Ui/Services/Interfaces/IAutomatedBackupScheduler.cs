namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupScheduler
{
    /// <summary>
    ///     Synchronizes operating system registration for nightly automated backups with the supplied enabled state.
    /// </summary>
    /// <param name="isEnabled">
    ///     <see langword="true" /> to ensure the nightly 11:30 PM local trigger is registered; otherwise,
    ///     <see langword="false" /> to remove future automated backup triggers.
    /// </param>
    void Synchronize(bool isEnabled);
}
