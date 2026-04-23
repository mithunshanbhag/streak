namespace Streak.Ui.Repositories.Interfaces;

/// <summary>
///     Represents the composite key used to identify a check-in by habit identifier and calendar date.
/// </summary>
/// <param name="HabitId">The habit identifier portion of the key.</param>
/// <param name="CheckinDate">The check-in date portion of the key.</param>
public readonly record struct CheckinKey(int HabitId, string CheckinDate);

/// <summary>
///     Provides persistence operations specific to <see cref="Checkin" /> entities.
/// </summary>
public interface ICheckinRepository : ISqlGenericRepository<Checkin, CheckinKey>
{
    /// <summary>
    ///     Retrieves all check-ins for a single habit ordered from newest to oldest date.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of check-ins for the habit, ordered by descending date.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="habitName" /> is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<Checkin>> GetByHabitNameAsync(string habitName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves check-ins for one or more habits, optionally filtered by an inclusive date range.
    /// </summary>
    /// <param name="habitNames">The habit names to include.</param>
    /// <param name="fromDate">The optional lower-bound date string.</param>
    /// <param name="toDate">The optional upper-bound date string.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of matching check-ins ordered by habit name and descending date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="habitNames" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when any habit name in <paramref name="habitNames" /> is null, empty, or
    ///     whitespace.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<Checkin>> GetByHabitNamesAsync(
        IReadOnlyCollection<string> habitNames,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves check-ins for one or more habit identifiers, optionally filtered by an inclusive date range.
    /// </summary>
    /// <param name="habitIds">The habit identifiers to include.</param>
    /// <param name="fromDate">The optional lower-bound date string.</param>
    /// <param name="toDate">The optional upper-bound date string.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of matching check-ins ordered by habit identifier and descending date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="habitIds" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when any habit identifier in <paramref name="habitIds" /> is less than or equal to zero.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<Checkin>> GetByHabitIdsAsync(
        IReadOnlyCollection<int> habitIds,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves check-in keys for one or more habit identifiers, optionally filtered by an inclusive date range.
    /// </summary>
    /// <param name="habitIds">The habit identifiers to include.</param>
    /// <param name="fromDate">The optional lower-bound date string.</param>
    /// <param name="toDate">The optional upper-bound date string.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of matching check-in keys ordered by habit identifier and descending date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="habitIds" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when any habit identifier in <paramref name="habitIds" /> is less than or equal to zero.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<CheckinKey>> GetKeysByHabitIdsAsync(
        IReadOnlyCollection<int> habitIds,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a check-in for a specific habit and date.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="checkinDate">The check-in date.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching check-in when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="habitName" /> or <paramref name="checkinDate" /> is
    ///     null, empty, or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when multiple check-ins match the supplied habit name and date.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Checkin?> GetByHabitNameAndDateAsync(string habitName, string checkinDate, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a check-in for a specific habit and date.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="checkinDate">The check-in date.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when a matching check-in was deleted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="habitName" /> or <paramref name="checkinDate" /> is
    ///     null, empty, or whitespace.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> DeleteByHabitNameAndDateAsync(string habitName, string checkinDate, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all check-ins for a specific habit.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when at least one check-in was deleted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="habitName" /> is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> DeleteByHabitNameAsync(string habitName, CancellationToken cancellationToken = default);
}
