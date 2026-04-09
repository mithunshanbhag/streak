using Microsoft.Azure.Cosmos;
using Nucleus.Repositories.Implementations;

namespace Streak.Core.Repositories.Implementations;

public sealed class HabitRepository2(Database cosmosDatabase)
    : CosmosGenericRepositoryBase<Habit2>(cosmosDatabase, CosmosConstants.ContainerNameHabits), IHabitRepository2;
