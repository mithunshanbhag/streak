namespace Streak.Ui.Repositories.Interfaces;

/// <summary>
///     Provides persistence operations specific to <see cref="Habit" /> entities.
/// </summary>
public interface IHabitRepository : ISqlGenericRepository<Habit, int>
{
    /// <summary>
    ///     Retrieves a habit by name after normalizing the supplied value.
    /// </summary>
    /// <param name="name">The habit name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching habit when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name" /> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when multiple habits match the supplied normalized name.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<Habit?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Determines whether a habit with the supplied name exists.
    /// </summary>
    /// <param name="name">The habit name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when a matching habit exists; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name" /> is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a habit by name.
    /// </summary>
    /// <param name="name">The habit name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when a matching habit was deleted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name" /> is null, empty, or whitespace.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> DeleteByNameAsync(string name, CancellationToken cancellationToken = default);
}