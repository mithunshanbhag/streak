using Streak.Core.Services.Implementations;
using Streak.Core.Services.Interfaces;

namespace Streak.Ui.Misc.ExtensionMethods;

public static class MauiAppBuilderExtensions
{
    extension(MauiAppBuilder builder)
    {
        public MauiAppBuilder ConfigureApp()
        {
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

            return builder;
        }

        public MauiAppBuilder ConfigureServices()
        {
            // automapper
            builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MapperProfile>(); });

            // mediatr
            builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); });

            // named http clients

            // mudblazor
            builder.Services.AddMudServices();

            // validators
            foreach (var validatorRegistration in AssemblyScanner.FindValidatorsInAssembly(Assembly.GetExecutingAssembly()))
                builder.Services.AddSingleton(validatorRegistration.InterfaceType, validatorRegistration.ValidatorType);

            var databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "streak.local.db");
            builder.Services.AddDbContext<StreakDbContext>(options => { options.UseSqlite($"Data Source={databasePath}"); });

            // repositories
            builder.Services.AddTransient<IHabitRepository, HabitRepository>();
            builder.Services.AddTransient<ICheckinRepository, CheckinRepository>();

            // services
            builder.Services.AddTransient<IHabitService, HabitService>();
            builder.Services.AddTransient<ICheckinService, CheckinService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder;
        }
    }
}