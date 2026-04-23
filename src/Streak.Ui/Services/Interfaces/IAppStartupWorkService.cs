namespace Streak.Ui.Services.Interfaces;

/// <summary>
///     Executes the one-time local startup work required before routed UI can access app data.
/// </summary>
public interface IAppStartupWorkService
{
    /// <summary>
    ///     Performs database bootstrap, schema upgrade, and startup scheduler synchronization.
    /// </summary>
    void Execute();
}
