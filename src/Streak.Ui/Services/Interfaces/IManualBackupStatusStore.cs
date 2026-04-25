namespace Streak.Ui.Services.Interfaces;

public interface IManualBackupStatusStore
{
    /// <summary>
    ///     Gets the last successful manual backup completion time for the specified location.
    /// </summary>
    /// <param name="location">The local or cloud manual backup location to read.</param>
    /// <returns>
    ///     The last successful completion timestamp in UTC, or <see langword="null" /> when no successful backup has been recorded.
    /// </returns>
    DateTimeOffset? GetLastSuccessfulBackupUtc(ManualBackupLocation location);

    /// <summary>
    ///     Persists the last successful manual backup completion time for the specified location.
    /// </summary>
    /// <param name="location">The local or cloud manual backup location to update.</param>
    /// <param name="completedAtUtc">The successful completion timestamp in UTC.</param>
    void SetLastSuccessfulBackupUtc(ManualBackupLocation location, DateTimeOffset completedAtUtc);

    /// <summary>
    ///     Clears any persisted manual backup completion time for the specified location.
    /// </summary>
    /// <param name="location">The local or cloud manual backup location to clear.</param>
    void Clear(ManualBackupLocation location);
}
