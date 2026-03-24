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

    protected override Expression<Func<Habit, bool>> BuildKeyPredicate(int key)
    {
        return x => x.Id == key;
    }
}