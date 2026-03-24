namespace Streak.Core.Repositories.Interfaces;

/// <summary>
///     Defines a generic repository contract for working with SQL-backed persistence models.
/// </summary>
/// <typeparam name="TEntity">The entity type persisted by the repository.</typeparam>
/// <typeparam name="TKey">The key type used to identify individual entities.</typeparam>
public interface ISqlGenericRepository<TEntity, in TKey> where TEntity : class
{
    /// <summary>
    ///     Retrieves all persisted entities.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list containing all persisted entities.</returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the entities matching the supplied keys. Keys that do not resolve to an entity are ignored.
    /// </summary>
    /// <param name="keys">The collection of keys to load.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of the entities that were found.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="keys" /> is <see langword="null" /> or contains a
    ///     <see langword="null" /> key.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when a supplied key resolves to multiple persisted entities.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<IReadOnlyList<TEntity>> GetByKeysAsync(IReadOnlyCollection<TKey> keys, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a single entity by key.
    /// </summary>
    /// <param name="key">The entity key.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching entity when it exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the supplied key resolves to multiple persisted entities.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the total number of persisted entities.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The total entity count.</returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Determines whether an entity with the supplied key exists.
    /// </summary>
    /// <param name="key">The entity key.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when a matching entity exists; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new entity to persistence.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when at least one change was persisted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a collection of entities to persistence.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when at least one change was persisted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> AddRangeAsync(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a single entity matching the supplied predicate.
    /// </summary>
    /// <param name="predicate">The predicate used to match an entity.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The matching entity when one exists; otherwise, <see langword="null" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the predicate matches multiple persisted entities.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<TEntity?> GetByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Determines whether any entity matches the supplied predicate.
    /// </summary>
    /// <param name="predicate">The predicate used to test for existence.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when at least one matching entity exists; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> ExistsByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all entities matching the supplied predicate.
    /// </summary>
    /// <param name="predicate">The predicate used to select entities for deletion.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when at least one entity was deleted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> DeleteByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing entity in persistence.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when at least one change was persisted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an entity by key.
    /// </summary>
    /// <param name="key">The entity key.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when an entity was deleted; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when the operation is canceled via
    ///     <paramref name="cancellationToken" />.
    /// </exception>
    Task<bool> DeleteAsync(TKey key, CancellationToken cancellationToken = default);
}