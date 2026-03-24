namespace Streak.Core.Repositories.Implementations;

public abstract class SqlGenericRepositoryBase<TEntity, TKey>(DbContext dbContext)
    : ISqlGenericRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly DbSet<TEntity> EntitySet = dbContext.Set<TEntity>();
    protected readonly DbContext StreakDbContext = dbContext;

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetByKeysAsync(
        IReadOnlyCollection<TKey> keys,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        if (keys.Count == 0) return [];

        List<TEntity> entities = [];
        foreach (var key in keys)
        {
            if (key is null) throw new ArgumentNullException(nameof(keys));

            var entity = await GetAsync(key, cancellationToken);
            if (entity is not null) entities.Add(entity);
        }

        return entities;
    }

    public Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken = default)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        return GetByPredicateAsync(BuildKeyPredicate(key), cancellationToken);
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return GetEntityCountAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken = default)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        return ExistsByPredicateAsync(BuildKeyPredicate(key), cancellationToken);
    }

    public Task<bool> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return AddEntityAsync(entity, nameof(entity), cancellationToken);
    }

    public async Task<bool> AddRangeAsync(
        IReadOnlyCollection<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0) return false;

        await EntitySet.AddRangeAsync(entities, cancellationToken);
        return await SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        EntitySet.Update(entity);
        return await SaveChangesAsync(cancellationToken) > 0;
    }

    public Task<bool> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        return DeleteByPredicateAsync(BuildKeyPredicate(key), cancellationToken);
    }

    public Task<TEntity?> GetByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Query().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<bool> ExistsByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Query().AnyAsync(predicate, cancellationToken);
    }

    public async Task<bool> DeleteByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var deletedCount = await EntitySet
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
            DetachTrackedEntities(predicate);

        return deletedCount > 0;
    }

    #region Helper Methods

    protected IQueryable<TEntity> Query(bool asNoTracking = true)
    {
        return asNoTracking ? EntitySet.AsNoTracking() : EntitySet;
    }

    protected abstract Expression<Func<TEntity, bool>> BuildKeyPredicate(TKey key);

    protected Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return StreakDbContext.SaveChangesAsync(cancellationToken);
    }

    protected static string NormalizeRequiredText(string value, string paramName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null or whitespace.", paramName)
            : value.Trim();
    }

    #endregion

    #region Private Helper Methods

    private Task<int> GetEntityCountAsync(CancellationToken cancellationToken = default)
    {
        return Query().CountAsync(cancellationToken);
    }

    private void DetachTrackedEntities(Expression<Func<TEntity, bool>> predicate)
    {
        var compiledPredicate = predicate.Compile();
        var trackedEntities = EntitySet.Local
            .Where(compiledPredicate)
            .ToArray();

        foreach (var trackedEntity in trackedEntities)
            StreakDbContext.Entry(trackedEntity).State = EntityState.Detached;
    }

    private async Task<bool> AddEntityAsync(
        TEntity entity,
        string paramName,
        CancellationToken cancellationToken = default)
    {
        if (entity is null) throw new ArgumentNullException(paramName);

        await EntitySet.AddAsync(entity, cancellationToken);
        return await SaveChangesAsync(cancellationToken) > 0;
    }

    #endregion
}