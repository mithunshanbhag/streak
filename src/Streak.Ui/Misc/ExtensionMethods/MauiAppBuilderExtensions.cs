using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Misc.ExtensionMethods;

public static class MauiAppBuilderExtensions
{
    extension(MauiAppBuilder builder)
    {
        public MauiAppBuilder ConfigureConfiguration()
        {
            ApplicationInsightsConfigurationUtility.AddAppSettings(builder.Configuration);

            return builder;
        }

        public MauiAppBuilder ConfigureApp()
        {
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

            return builder;
        }

        public MauiAppBuilder ConfigureServices()
        {
#if DEBUG
            ApplicationInsightsLoggingUtility.Configure(builder.Logging, builder.Configuration, isDebugBuild: true);
#else
            ApplicationInsightsLoggingUtility.Configure(builder.Logging, builder.Configuration, isDebugBuild: false);
#endif

            // named http clients

            // mudblazor
            builder.Services.AddMudServices();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();

            // validators
            foreach (var validatorAssembly in new[] { Assembly.GetExecutingAssembly(), typeof(StreakDbContext).Assembly }.Distinct())
                foreach (var validatorRegistration in AssemblyScanner.FindValidatorsInAssembly(validatorAssembly))
                    builder.Services.AddSingleton(validatorRegistration.InterfaceType, validatorRegistration.ValidatorType);

            builder.Services.AddSingleton<SqliteDatabaseBootstrapper>();
            builder.Services.AddSingleton<SqliteDatabaseSchemaUpgrader>();
            builder.Services.AddSingleton<IAppStartupWorkService, AppStartupWorkService>();
            builder.Services.AddSingleton<IAppInitializationService, AppInitializationService>();
            builder.Services.AddSingleton<IPostStartupPermissionRecoveryCoordinator, PostStartupPermissionRecoveryCoordinator>();
            builder.Services.AddDbContext<StreakDbContext>(options => { options.UseSqlite(SqliteDatabaseBootstrapper.ConnectionString); });

            // repositories
            builder.Services.AddTransient<IHabitRepository, HabitRepository>();
            builder.Services.AddTransient<ICheckinRepository, CheckinRepository>();

            // services
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton<IAppInfo>(_ => AppInfo.Current);
            builder.Services.AddSingleton<IPreferences>(_ => Preferences.Default);
            builder.Services.AddSingleton<IAppVersionInfoService, AppVersionInfoService>();
            builder.Services.AddSingleton<IOneDriveAuthConfigurationProvider, OneDriveAuthConfigurationProvider>();
            builder.Services.AddSingleton<IOneDriveAuthReturnRouteStore, OneDriveAuthReturnRouteStore>();
            builder.Services.AddSingleton<IOneDriveAuthStateStore, OneDriveAuthStateStore>();
            builder.Services.AddSingleton<IManualBackupStatusStore, ManualBackupStatusStore>();
            builder.Services.AddScoped<StreakAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(services =>
                services.GetRequiredService<StreakAuthenticationStateProvider>());
            builder.Services.AddTransient<IHabitService, HabitService>();
            builder.Services.AddTransient<ICheckinService, CheckinService>();
            builder.Services.AddSingleton<IAppStoragePathService, AppStoragePathService>();
            builder.Services.AddSingleton<ICheckinProofFileStore, FileSystemCheckinProofFileStore>();
            builder.Services.AddTransient<IBackupArchiveFactory, BackupArchiveFactory>();
            builder.Services.AddSingleton<ICheckinProofService, CheckinProofService>();
            builder.Services.AddSingleton<IAutomatedBackupConfigurationService, AutomatedBackupConfigurationService>();
            builder.Services.AddSingleton<IReminderConfigurationService, ReminderConfigurationService>();
            builder.Services.AddSingleton<IReminderNotificationPermissionCoordinator, ReminderNotificationPermissionCoordinator>();
            builder.Services.AddTransient<IManualBackupCompletionNotifier, ManualBackupCompletionNotifier>();
            builder.Services.AddTransient<IManualCloudBackupService, UnsupportedManualCloudBackupService>();
            builder.Services.AddTransient<IAutomatedCloudBackupService, UnsupportedAutomatedCloudBackupService>();
            builder.Services.AddTransient<IDatabaseImportService, DatabaseImportService>();
            builder.Services.AddTransient<IDatabaseExportService, DatabaseExportService>();
#if WINDOWS
            builder.Services.AddSingleton<IAutomatedBackupScheduler, NoOpAutomatedBackupScheduler>();
            builder.Services.AddSingleton<IReminderScheduler, NoOpReminderScheduler>();
            builder.Services.AddSingleton<IBackupFolderOpener, WindowsBackupFolderOpener>();
            builder.Services.AddSingleton<IAutomatedBackupCompletionNotifier, NoOpAutomatedBackupCompletionNotifier>();
            builder.Services.AddSingleton<IBackupNotificationPermissionService, NoOpBackupNotificationPermissionService>();
            builder.Services.AddSingleton<ICameraPermissionService, NoOpCameraPermissionService>();
            builder.Services.AddSingleton<IReminderNotifier, NoOpReminderNotifier>();
            builder.Services.AddSingleton<IReminderNotificationPermissionService, NoOpReminderNotificationPermissionService>();
            builder.Services.AddSingleton<IOneDriveAuthService, UnsupportedOneDriveAuthService>();
            builder.Services.AddTransient<IOneDriveBackupUploadClient, UnsupportedOneDriveBackupUploadClient>();
            builder.Services.AddTransient<IDatabaseImportFilePicker, WindowsDatabaseImportFilePicker>();
            builder.Services.AddTransient<ICheckinProofMediaPickerService, WindowsCheckinProofMediaPickerService>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, WindowsDatabaseExportFileSaver>();
            builder.Services.AddTransient<IDatabaseShareService, UnsupportedDatabaseShareService>();
#elif ANDROID
            builder.Services.AddSingleton<ICheckinProofFileStore, AndroidCheckinProofFileStore>();
            builder.Services.AddSingleton<IBackupFolderOpener, AndroidBackupFolderOpener>();
            builder.Services.AddSingleton<IAutomatedBackupCompletionNotifier, AndroidAutomatedBackupCompletionNotifier>();
            builder.Services.AddSingleton<IBackupNotificationPermissionService, AndroidBackupNotificationPermissionService>();
            builder.Services.AddSingleton<ICameraPermissionService, AndroidCameraPermissionService>();
            builder.Services.AddSingleton<IReminderNotifier, AndroidReminderNotifier>();
            builder.Services.AddSingleton<IReminderNotificationPermissionService, AndroidReminderNotificationPermissionService>();
            builder.Services.AddTransient<IDatabaseImportFilePicker, AndroidDatabaseImportFilePicker>();
            builder.Services.AddTransient<ICheckinProofMediaPickerService, AndroidCheckinProofMediaPickerService>();
            builder.Services.AddTransient<IDatabaseExportFileSaver, AndroidDatabaseExportFileSaver>();
            builder.Services.AddTransient<IAutomatedBackupFileSaver, AndroidAutomatedBackupFileSaver>();
            builder.Services.AddTransient<IAutomatedBackupExecutionService, AutomatedBackupExecutionService>();
            builder.Services.AddTransient<IAutomatedBackupRunService, AutomatedBackupRunService>();
            builder.Services.AddSingleton<IShare>(_ => Share.Default);
            builder.Services.AddSingleton<IAutomatedBackupScheduler, AndroidAutomatedBackupScheduler>();
            builder.Services.AddSingleton<IReminderScheduler, AndroidReminderScheduler>();
            builder.Services.AddSingleton<IOneDriveAuthService, Streak.Ui.Platforms.Android.Services.AndroidOneDriveAuthService>();
            builder.Services.AddTransient<IManualCloudBackupService, ManualCloudBackupService>();
            builder.Services.AddTransient<IAutomatedCloudBackupService, AutomatedCloudBackupService>();
            builder.Services.AddHttpClient<IOneDriveBackupUploadClient, OneDriveBackupUploadClient>(
                client =>
                {
                    client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
                    client.Timeout = TimeSpan.FromMinutes(2);
                });
            builder.Services.AddTransient<IDatabaseShareService, DatabaseShareService>();
#else
            builder.Services.AddSingleton<IAutomatedBackupScheduler, NoOpAutomatedBackupScheduler>();
            builder.Services.AddSingleton<IReminderScheduler, NoOpReminderScheduler>();
            builder.Services.AddSingleton<IBackupFolderOpener, UnsupportedBackupFolderOpener>();
            builder.Services.AddSingleton<IAutomatedBackupCompletionNotifier, NoOpAutomatedBackupCompletionNotifier>();
            builder.Services.AddSingleton<IBackupNotificationPermissionService, NoOpBackupNotificationPermissionService>();
            builder.Services.AddSingleton<ICameraPermissionService, NoOpCameraPermissionService>();
            builder.Services.AddSingleton<IReminderNotifier, NoOpReminderNotifier>();
            builder.Services.AddSingleton<IReminderNotificationPermissionService, NoOpReminderNotificationPermissionService>();
            builder.Services.AddSingleton<IOneDriveAuthService, UnsupportedOneDriveAuthService>();
            builder.Services.AddTransient<IAutomatedCloudBackupService, UnsupportedAutomatedCloudBackupService>();
            builder.Services.AddTransient<IOneDriveBackupUploadClient, UnsupportedOneDriveBackupUploadClient>();
            builder.Services.AddTransient<ICheckinProofMediaPickerService, UnsupportedCheckinProofMediaPickerService>();
            builder.Services.AddTransient<IDatabaseShareService, UnsupportedDatabaseShareService>();
#endif

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            return builder;
        }
    }
}
