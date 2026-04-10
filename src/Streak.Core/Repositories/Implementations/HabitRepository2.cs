namespace Streak.Core.Repositories.Implementations;

public sealed class HabitRepository2(Database cosmosDatabase)
    : CosmosGenericRepositoryBase<Habit2>(cosmosDatabase, CosmosConstants.ContainerNameHabits), IHabitRepository2;