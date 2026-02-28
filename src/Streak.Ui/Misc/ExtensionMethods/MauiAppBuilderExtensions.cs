using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Streak.Ui.Repositories.Implementations;
using Streak.Ui.Repositories.Implementations.Sqlite;
using Streak.Ui.Repositories.Interfaces;
using Streak.Ui.Services.Implementations;
using Streak.Ui.Services.Interfaces;

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
            builder.Services.AddTransient<ICoreRepository, CoreRepository>();
            builder.Services.AddTransient<ICoreAppService, CoreAppService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder;
        }
    }
}