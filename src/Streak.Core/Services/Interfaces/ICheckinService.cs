namespace Streak.Core.Services.Interfaces;

/// <summary>
///     Provides operations for reading, mutating, and analyzing daily habit check-ins.
/// </summary>
public interface ICheckinService
{
    /// <summary>
    ///     Retrieves a check-in for a specific habit and calendar date.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="checkinDate">The check-in date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching check-in when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="habitName" /> or <paramref name="checkinDate" /> is
    ///     invalid.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Checkin?> GetByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves check-in history for a habit within an optional inclusive date range.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="fromDate">The optional lower-bound date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="toDate">The optional upper-bound date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of matching check-ins.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="habitName" />, <paramref name="fromDate" />, or
    ///     <paramref name="toDate" /> is invalid, or when <paramref name="fromDate" /> is later than
    ///     <paramref name="toDate" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<Checkin>> GetHistoryAsync(
        string habitName,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the homepage card state for all habits using the device's current local day.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A read-only list of homepage habit-card view models ordered alphabetically by habit name.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<HabitCheckinViewModel>> GetHomePageHabitCheckinsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new check-in or returns the existing check-in for the same habit and date.
    /// </summary>
    /// <param name="checkin">The check-in to add or update.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The persisted check-in instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="checkin" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when the check-in date is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="Checkin.HabitId" /> is less than or equal to zero.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the related habit does not exist or when persistence fails.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Checkin> UpsertAsync(Checkin checkin, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or removes today's check-in for a habit.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="isDone"><see langword="true" /> to mark the habit done for today; otherwise, <see langword="false" />.</param>
    /// <param name="notes">
    ///     The optional note to persist with today's check-in when <paramref name="isDone" /> is
    ///     <see langword="true" />. Ignored when <paramref name="isDone" /> is <see langword="false" />.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     The persisted check-in for today when <paramref name="isDone" /> is <see langword="true" />; otherwise,
    ///     <see langword="null" />.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="habitName" /> is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the related habit does not exist or when persistence fails.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Checkin?> ToggleForTodayAsync(
        string habitName,
        bool isDone,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a check-in for a specific habit and date.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="checkinDate">The check-in date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="habitName" /> or <paramref name="checkinDate" /> is
    ///     invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the related habit does not exist, when the check-in does not
    ///     exist, or when the delete operation fails.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task DeleteForHabitAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Calculates the current consecutive-day streak for a habit based on its check-in history.
    /// </summary>
    /// <param name="habitName">The habit name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The current streak length. Returns 0 when the habit has no qualifying consecutive check-ins.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="habitName" /> is invalid.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<int> GetCurrentStreakAsync(string habitName, CancellationToken cancellationToken = default);
}
