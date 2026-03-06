namespace Streak.Core.Repositories.Interfaces;

public interface ISqlGenericRepository<TEntity, in TKey> where TEntity : class
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetByKeysAsync(IReadOnlyCollection<TKey> keys, CancellationToken cancellationToken = default);

    Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken = default);

    Task<bool> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> AddRangeAsync(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(TKey key, CancellationToken cancellationToken = default);
}