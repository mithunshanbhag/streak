using Microsoft.Extensions.DependencyInjection;
using Serilog;

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
            builder.Services.AddSingleton<ICheckinProofFileStore, FileSystemCheckinProofFileStore>();
            builder.Services.AddSingleton<ICheckinProofService, CheckinProofService>();
            builder.Services.AddSingleton<IAutomatedBackupConfigurationService, AutomatedBackupConfigurationService>();
            builder.Services.AddSingleton<IReminderConfigurationService, ReminderConfigurationService>();
            builder.Services.AddTransient<IManualBackupCompletionNotifier, ManualBackupCompletionNotifier>();
            builder.Services.AddTransient<IDatabaseImportService, DatabaseImportService>();
            builder.Services.AddTransient<IDatabaseExportService, DatabaseExportService>();
            builder.Services.AddTransient<IDiagnosticsExportService, DiagnosticsExportService>();
#if WINDOWS
            builder.Services.AddSingleton<IAutomatedBackupScheduler, NoOpAutomatedBackupScheduler>();
            builder.Services.AddSingleton<IReminderScheduler, NoOpReminderScheduler>();
            builder.Services.AddSingleton<IBackupFolderOpener, WindowsBackupFolderOpener>();
            builder.Services.AddSingleton<IAutomatedBackupCompletionNotifier, NoOpAutomatedBackupCompletionNotifier>();
            builder.Services.AddSingleton<IBackupNotificationPermissionService, NoOpBackupNotificationPermissionService>();
            builder.Services.AddSingleton<IReminderNotifier, NoOpReminderNotifier>();
            builder.Services.AddSingleton<IReminderNotificationPermissionService, NoOpReminderNotificationPermissionService>();
            builder.Services.AddTransient<IDatabaseImportFilePicker, WindowsDatabaseImportFilePicker>();
            builder.Services.AddTransient<ICheckinProofMediaPickerService, WindowsCheckinProofMediaPickerService>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, WindowsDatabaseExportFileSaver>();
            builder.Services.AddTransient<IDiagnosticsExportFileSaver, WindowsDiagnosticsExportFileSaver>();
            builder.Services.AddTransient<IDatabaseShareService, UnsupportedDatabaseShareService>();
#elif ANDROID
            builder.Services.AddSingleton<ICheckinProofFileStore, AndroidCheckinProofFileStore>();
            builder.Services.AddSingleton<IBackupFolderOpener, AndroidBackupFolderOpener>();
            builder.Services.AddSingleton<IAutomatedBackupCompletionNotifier, AndroidAutomatedBackupCompletionNotifier>();
            builder.Services.AddSingleton<IBackupNotificationPermissionService, AndroidBackupNotificationPermissionService>();
            builder.Services.AddSingleton<IReminderNotifier, AndroidReminderNotifier>();
            builder.Services.AddSingleton<IReminderNotificationPermissionService, AndroidReminderNotificationPermissionService>();
            builder.Services.AddTransient<IDatabaseImportFilePicker, AndroidDatabaseImportFilePicker>();
            builder.Services.AddTransient<ICheckinProofMediaPickerService, AndroidCheckinProofMediaPickerService>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, AndroidDatabaseExportFileSaver>();
            builder.Services.AddTransient<IDiagnosticsExportFileSaver, AndroidDiagnosticsExportFileSaver>();
            builder.Services.AddTransient<IAutomatedBackupFileSaver, AndroidAutomatedBackupFileSaver>();
            builder.Services.AddTransient<IAutomatedBackupExecutionService, AutomatedBackupExecutionService>();
            builder.Services.AddSingleton<IShare>(_ => Share.Default);
            builder.Services.AddSingleton<IAutomatedBackupScheduler, AndroidAutomatedBackupScheduler>();
            builder.Services.AddSingleton<IReminderScheduler, AndroidReminderScheduler>();
            builder.Services.AddTransient<IDatabaseShareService, DatabaseShareService>();
#else
            builder.Services.AddSingleton<IAutomatedBackupScheduler, NoOpAutomatedBackupScheduler>();
            builder.Services.AddSingleton<IReminderScheduler, NoOpReminderScheduler>();
            builder.Services.AddSingleton<IBackupFolderOpener, UnsupportedBackupFolderOpener>();
            builder.Services.AddSingleton<IAutomatedBackupCompletionNotifier, NoOpAutomatedBackupCompletionNotifier>();
            builder.Services.AddSingleton<IBackupNotificationPermissionService, NoOpBackupNotificationPermissionService>();
            builder.Services.AddSingleton<IReminderNotifier, NoOpReminderNotifier>();
            builder.Services.AddSingleton<IReminderNotificationPermissionService, NoOpReminderNotificationPermissionService>();
            builder.Services.AddTransient<IDiagnosticsExportFileSaver, WindowsDiagnosticsExportFileSaver>();
            builder.Services.AddTransient<ICheckinProofMediaPickerService, UnsupportedCheckinProofMediaPickerService>();
            builder.Services.AddTransient<IDatabaseShareService, UnsupportedDatabaseShareService>();
#endif

            builder.Services.AddMauiBlazorWebView();
            builder.Logging.ClearProviders();
            builder.Services.AddSerilog((services, loggerConfiguration) =>
            {
                var appStoragePathService = services.GetRequiredService<IAppStoragePathService>();

                loggerConfiguration
                    .MinimumLevel.Is(GetDefaultLogLevel())
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Sink(
                        new CircularFileSink(
                            appStoragePathService.DiagnosticsLogFilePath,
                            new Serilog.Formatting.Compact.CompactJsonFormatter(),
                            DiagnosticsConstants.MaxStructuredLogFileSizeBytes));

#if DEBUG
                loggerConfiguration.WriteTo.Debug();
#endif
            });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            return builder;
        }
    }

    private static Serilog.Events.LogEventLevel GetDefaultLogLevel()
    {
#if DEBUG
        return Serilog.Events.LogEventLevel.Debug;
#else
        return Serilog.Events.LogEventLevel.Information;
#endif
    }
}
