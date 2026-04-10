using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Api.Misc.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStreakApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddMvc();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddSingleton(_ =>
        {
            var connectionString = configuration[ConfigKeys.CosmosConnectionString];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    $"Configuration value '{ConfigKeys.CosmosConnectionString}' is required.");

            return new CosmosClient(connectionString);
        });

        services.AddSingleton(sp =>
        {
            var databaseName = configuration[ConfigKeys.CosmosDatabaseName];
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException(
                    $"Configuration value '{ConfigKeys.CosmosDatabaseName}' is required.");

            return sp.GetRequiredService<CosmosClient>().GetDatabase(databaseName);
        });

        services.AddScoped<IHabitRepository2, HabitRepository2>();
        services.AddScoped<ICheckinRepository2, CheckinRepository2>();
        services.AddScoped<IHabitService2, HabitService2>();
        services.AddScoped<ICheckinService2, CheckinService2>();

        services.AddScoped<IValidator<CreateHabitRequestDto>, CreateHabitRequestDtoValidator>();
        services.AddScoped<IValidator<UpdateHabitRequestDto>, UpdateHabitRequestDtoValidator>();

        return services;
    }
}
