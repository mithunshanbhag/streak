namespace Streak.Core.Repositories.Interfaces;

public interface IHabitRepository : ISqlGenericRepository<Habit, int>
{
    Task<Habit?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> DeleteByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> ReorderAsync(
        IReadOnlyList<Habit> habitsInDisplayOrder,
        CancellationToken cancellationToken = default);
}