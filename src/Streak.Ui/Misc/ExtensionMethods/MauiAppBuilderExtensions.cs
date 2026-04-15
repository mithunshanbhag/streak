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
            builder.Services.AddSingleton<SqliteDatabaseSchemaUpgrader>();
            builder.Services.AddDbContext<StreakDbContext>(options => { options.UseSqlite(SqliteDatabaseBootstrapper.ConnectionString); });

            // repositories
            builder.Services.AddTransient<IHabitRepository, HabitRepository>();
            builder.Services.AddTransient<ICheckinRepository, CheckinRepository>();

            // services
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddTransient<IHabitService, HabitService>();
            builder.Services.AddTransient<ICheckinService, CheckinService>();
            builder.Services.AddSingleton<IAppStoragePathService, AppStoragePathService>();
            builder.Services.AddSingleton<IAutomatedBackupConfigurationService, AutomatedBackupConfigurationService>();
            builder.Services.AddTransient<IDatabaseImportService, DatabaseImportService>();
            builder.Services.AddTransient<IDatabaseExportService, DatabaseExportService>();
#if WINDOWS
            builder.Services.AddSingleton<IAutomatedBackupScheduler, NoOpAutomatedBackupScheduler>();
            builder.Services.AddTransient<IDatabaseImportFilePicker, WindowsDatabaseImportFilePicker>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, WindowsDatabaseExportFileSaver>();
            builder.Services.AddTransient<IDatabaseShareService, UnsupportedDatabaseShareService>();
#elif ANDROID
            builder.Services.AddTransient<IDatabaseImportFilePicker, AndroidDatabaseImportFilePicker>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, AndroidDatabaseExportFileSaver>();
            builder.Services.AddSingleton<IShare>(_ => Share.Default);
            builder.Services.AddSingleton<IAutomatedBackupScheduler, AndroidAutomatedBackupScheduler>();
            builder.Services.AddTransient<IDatabaseShareService, DatabaseShareService>();
#else
            builder.Services.AddSingleton<IAutomatedBackupScheduler, NoOpAutomatedBackupScheduler>();
            builder.Services.AddTransient<IDatabaseShareService, UnsupportedDatabaseShareService>();
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
