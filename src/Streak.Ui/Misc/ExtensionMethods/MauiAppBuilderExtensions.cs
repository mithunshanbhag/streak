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
            // named http clients

            // mudblazor
            builder.Services.AddMudServices();

            // validators
            foreach (var validatorAssembly in new[] { Assembly.GetExecutingAssembly(), typeof(StreakDbContext).Assembly }.Distinct())
                foreach (var validatorRegistration in AssemblyScanner.FindValidatorsInAssembly(validatorAssembly))
                    builder.Services.AddSingleton(validatorRegistration.InterfaceType, validatorRegistration.ValidatorType);

            builder.Services.AddSingleton<SqliteDatabaseBootstrapper>();
            builder.Services.AddDbContext<StreakDbContext>(options => { options.UseSqlite(SqliteDatabaseBootstrapper.ConnectionString); });

            // repositories
            builder.Services.AddTransient<IHabitRepository, HabitRepository>();
            builder.Services.AddTransient<ICheckinRepository, CheckinRepository>();

            // services
            builder.Services.AddTransient<IHabitService, HabitService>();
            builder.Services.AddTransient<ICheckinService, CheckinService>();
            builder.Services.AddTransient<IAppStoragePathService, AppStoragePathService>();
            builder.Services.AddTransient<IDatabaseImportService, DatabaseImportService>();
            builder.Services.AddTransient<IDatabaseExportService, DatabaseExportService>();
#if WINDOWS
            builder.Services.AddTransient<IDatabaseImportFilePicker, WindowsDatabaseImportFilePicker>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, WindowsDatabaseExportFileSaver>();
#elif ANDROID
            builder.Services.AddTransient<IDatabaseImportFilePicker, AndroidDatabaseImportFilePicker>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, AndroidDatabaseExportFileSaver>();
#endif

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder;
        }
    }
}
