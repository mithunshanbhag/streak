namespace Streak.Core.Repositories.Implementations;

public class HabitRepository(StreakDbContext dbContext) : SqlGenericRepositoryBase<Habit, int>(dbContext), IHabitRepository
{
    public async Task<Habit?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeRequiredText(name, nameof(name));
        return await GetByPredicateAsync(x => x.Name == normalizedName, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeRequiredText(name, nameof(name));
        return await ExistsByPredicateAsync(x => x.Name == normalizedName, cancellationToken);
    }

    public async Task<bool> DeleteByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeRequiredText(name, nameof(name));

        return await DeleteByPredicateAsync(x => x.Name == normalizedName, cancellationToken);
    }

    public async Task<bool> ReorderAsync(
        IReadOnlyList<Habit> habitsInDisplayOrder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(habitsInDisplayOrder);

        if (habitsInDisplayOrder.Count == 0) return false;

        var habitIds = habitsInDisplayOrder.Select(x => x.Id).ToArray();
        var habitsById = await EntitySet
            .Where(x => habitIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (habitsById.Count != habitIds.Length) return false;

        var maxDisplayOrder = await EntitySet
            .MaxAsync(x => (int?)x.DisplayOrder, cancellationToken) ?? 0;
        var temporaryDisplayOrderStart = maxDisplayOrder + habitIds.Length + 1;

        await using var transaction = await StreakDbContext.Database.BeginTransactionAsync(cancellationToken);

        for (var index = 0; index < habitIds.Length; index++) habitsById[habitIds[index]].DisplayOrder = temporaryDisplayOrderStart + index;

        var temporaryRowsUpdated = await SaveChangesAsync(cancellationToken);
        if (temporaryRowsUpdated == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        for (var index = 0; index < habitIds.Length; index++) habitsById[habitIds[index]].DisplayOrder = index + 1;

        var finalRowsUpdated = await SaveChangesAsync(cancellationToken);
        if (finalRowsUpdated == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    protected override Expression<Func<Habit, bool>> BuildKeyPredicate(int key)
    {
        return x => x.Id == key;
    }
}