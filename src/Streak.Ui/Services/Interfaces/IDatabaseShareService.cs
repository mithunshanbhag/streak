namespace Streak.Ui.Services.Interfaces;

public interface IDatabaseShareService
{
    /// <summary>
    /// Gets a value indicating whether database sharing is supported on the current platform.
    /// </summary>
    bool CanShare { get; }

    /// <summary>
    /// Creates a backup copy of the local database and opens the operating system share flow for it.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task ShareDatabaseAsync(CancellationToken cancellationToken = default);
}
