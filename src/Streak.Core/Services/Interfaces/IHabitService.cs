namespace Streak.Core.Services.Interfaces;

public interface IHabitService
{
    Task<IReadOnlyList<Habit>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Habit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Habit?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default);

    Task<Habit> UpdateAsync(Habit habit, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task ReorderAsync(IReadOnlyList<int> habitIdsInDisplayOrder, CancellationToken cancellationToken = default);
}