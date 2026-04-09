using Microsoft.Azure.Cosmos;
using Nucleus.Repositories.Implementations;

namespace Streak.Core.Repositories.Implementations;

public sealed class CheckinRepository2(Database cosmosDatabase)
    : CosmosGenericRepositoryBase<Checkin2>(cosmosDatabase, CosmosConstants.ContainerNameCheckins), ICheckinRepository2;
