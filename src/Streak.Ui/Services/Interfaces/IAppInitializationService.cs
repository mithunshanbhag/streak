namespace Streak.Ui.Services.Interfaces;

/// <summary>
///     Coordinates one-time application startup work that must finish before routed UI accesses local data.
/// </summary>
public interface IAppInitializationService
{
    /// <summary>
    ///     Ensures the application's startup initialization has completed, starting it if needed.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to stop waiting for the initialization task.</param>
    Task EnsureInitializedAsync(CancellationToken cancellationToken = default);
}
