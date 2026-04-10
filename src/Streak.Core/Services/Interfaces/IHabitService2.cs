namespace Streak.Core.Services.Interfaces;

/// <summary>
///     Provides owner-scoped operations for creating, retrieving, updating, and deleting Cosmos-backed habits.
/// </summary>
public interface IHabitService2
{
    /// <summary>
    ///     Retrieves all habits for an owner sorted alphabetically by name.
    /// </summary>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of habits sorted by name.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ownerId" /> is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<Habit2>> GetAllAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a habit by owner and identifier.
    /// </summary>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="id">The habit identifier.</param>
    /// <param name="throwIfNotExists">
    ///     <see langword="true" /> to throw when the habit does not exist; otherwise,
    ///     <see langword="false" /> to return <see langword="null" />.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching habit when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="ownerId" /> or <paramref name="id" /> is null, empty,
    ///     or whitespace.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when the habit does not exist and <paramref name="throwIfNotExists" /> is
    ///     <see langword="true" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit2?> GetByIdAsync(
        string ownerId,
        string id,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a habit by owner and name using a case-insensitive comparison after normalizing the supplied value.
    /// </summary>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="name">The habit name.</param>
    /// <param name="throwIfNotExists">
    ///     <see langword="true" /> to throw when the habit does not exist; otherwise,
    ///     <see langword="false" /> to return <see langword="null" />.
    /// </param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching habit when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="ownerId" /> or <paramref name="name" /> is invalid.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when the habit does not exist and <paramref name="throwIfNotExists" /> is
    ///     <see langword="true" />.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit2?> GetByNameAsync(
        string ownerId,
        string name,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the total number of habits for an owner.
    /// </summary>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The total number of habits for the owner.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ownerId" /> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the returned count exceeds <see cref="int.MaxValue" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<int> GetCountAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new owner-scoped habit after normalizing and validating the supplied values.
    /// </summary>
    /// <param name="habit">The habit to create.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The created habit. If the incoming identifier is blank, a new identifier is generated.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="habit" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when the habit data is invalid.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the maximum habit count has been reached or a duplicate habit
    ///     identifier or name already exists.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit2> CreateAsync(Habit2 habit, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing owner-scoped habit after normalizing and validating the supplied values.
    /// </summary>
    /// <param name="habit">The habit to update.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The updated habit.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="habit" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when the habit data is invalid.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the target habit does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when another habit already uses the same normalized name.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit2> UpdateAsync(Habit2 habit, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a habit by owner and identifier.
    /// </summary>
    /// <param name="ownerId">The owner identifier.</param>
    /// <param name="id">The habit identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="ownerId" /> or <paramref name="id" /> is null, empty,
    ///     or whitespace.
    /// </exception>
    /// <exception cref="KeyNotFoundException">Thrown when the target habit does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the delete operation fails.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task DeleteAsync(string ownerId, string id, CancellationToken cancellationToken = default);
}