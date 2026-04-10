namespace Streak.Core.Services.Interfaces;

/// <summary>
///     Provides operations for reading, mutating, and analyzing Cosmos-backed habit check-ins.
/// </summary>
public interface ICheckinService2
{
    /// <summary>
    ///     Retrieves a check-in for a specific habit and calendar date.
    /// </summary>
    /// <param name="habitId">The habit identifier, which also acts as the partition key.</param>
    /// <param name="checkinDate">The check-in date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="throwIfNotExists">
    ///     <see langword="true" /> to throw when the check-in does not exist; otherwise,
    ///     <see langword="false" /> to return <see langword="null" />.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching check-in when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="habitId" /> or <paramref name="checkinDate" /> is invalid.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when the check-in does not exist and <paramref name="throwIfNotExists" />
    ///     is <see langword="true" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Checkin2?> GetByHabitIdAndDateAsync(
        string habitId,
        string checkinDate,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves check-in history for a habit within an optional inclusive date range.
    /// </summary>
    /// <param name="habitId">The habit identifier, which also acts as the partition key.</param>
    /// <param name="fromDate">The optional lower-bound date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="toDate">The optional upper-bound date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of matching check-ins ordered from newest to oldest date.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="habitId" />, <paramref name="fromDate" />, or
    ///     <paramref name="toDate" /> is invalid, or when <paramref name="fromDate" /> is later than
    ///     <paramref name="toDate" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<Checkin2>> GetHistoryAsync(
        string habitId,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new check-in or returns the existing check-in for the same habit and date.
    /// </summary>
    /// <param name="checkin">The check-in to persist.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The persisted check-in instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="checkin" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when the check-in data is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the related habit does not exist.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Checkin2> UpsertAsync(Checkin2 checkin, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or removes today's check-in for a habit.
    /// </summary>
    /// <param name="ownerId">The owner identifier used to validate the target habit.</param>
    /// <param name="habitId">The habit identifier, which also acts as the partition key.</param>
    /// <param name="isDone"><see langword="true" /> to mark the habit done for today; otherwise, <see langword="false" />.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     The persisted check-in for today when <paramref name="isDone" /> is <see langword="true" />; otherwise,
    ///     <see langword="null" />.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ownerId" /> or <paramref name="habitId" /> is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the related habit does not exist.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Checkin2?> ToggleForTodayAsync(
        string ownerId,
        string habitId,
        bool isDone,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a check-in for a specific habit and date.
    /// </summary>
    /// <param name="ownerId">The owner identifier used to validate the target habit.</param>
    /// <param name="habitId">The habit identifier, which also acts as the partition key.</param>
    /// <param name="checkinDate">The check-in date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="ownerId" />, <paramref name="habitId" />, or
    ///     <paramref name="checkinDate" /> is invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the related habit does not exist or when the check-in does not exist.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task DeleteForHabitAndDateAsync(
        string ownerId,
        string habitId,
        string checkinDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Calculates the current consecutive-day streak for a habit based on its check-in history.
    /// </summary>
    /// <param name="habitId">The habit identifier, which also acts as the partition key.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The current streak length. Returns 0 when the habit has no qualifying consecutive check-ins.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="habitId" /> is invalid.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<int> GetCurrentStreakAsync(string habitId, CancellationToken cancellationToken = default);
}