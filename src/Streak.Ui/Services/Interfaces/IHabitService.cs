namespace Streak.Ui.Services.Interfaces;

/// <summary>
///     Provides operations for creating, retrieving, updating, and deleting habits.
/// </summary>
public interface IHabitService
{
    /// <summary>
    ///     Retrieves all habits sorted alphabetically by name.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of habits sorted by name.</returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<Habit>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a habit by its identifier.
    /// </summary>
    /// <param name="id">The habit identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching habit when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is less than or equal to zero.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a habit by name using a case-insensitive comparison after normalizing the supplied value.
    /// </summary>
    /// <param name="name">The habit name to search for.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching habit when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="name" /> is null, empty, whitespace, or outside the
    ///     allowed length range.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the total number of habits.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The total number of habits.</returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new habit after normalizing and validating the supplied values.
    /// </summary>
    /// <param name="habit">The habit to create.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The created habit with its assigned identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="habit" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the habit name is null, empty, whitespace, or outside the allowed
    ///     length range.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the maximum habit count has been reached, when a duplicate
    ///     habit name already exists, or when persistence fails.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing habit after normalizing and validating the supplied values.
    /// </summary>
    /// <param name="habit">The habit values to persist.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The updated persisted habit instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="habit" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the habit name is null, empty, whitespace, or outside the allowed
    ///     length range.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the habit identifier is less than or equal to zero.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the target habit does not exist.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when another habit already uses the same name or when persistence
    ///     fails.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit> UpdateAsync(Habit habit, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a habit by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the habit to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is less than or equal to zero.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the target habit does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the delete operation fails.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}