namespace Streak.Ui.Services.Implementations;

public sealed class AppStartupWorkService(
    IAutomatedBackupConfigurationService automatedBackupConfigurationService,
    IReminderConfigurationService reminderConfigurationService,
    SqliteDatabaseBootstrapper sqliteDatabaseBootstrapper,
    SqliteDatabaseSchemaUpgrader sqliteDatabaseSchemaUpgrader,
    ILogger<AppStartupWorkService> logger)
    : IAppStartupWorkService
{
    private readonly IAutomatedBackupConfigurationService _automatedBackupConfigurationService = automatedBackupConfigurationService;
    private readonly IReminderConfigurationService _reminderConfigurationService = reminderConfigurationService;
    private readonly SqliteDatabaseBootstrapper _sqliteDatabaseBootstrapper = sqliteDatabaseBootstrapper;
    private readonly SqliteDatabaseSchemaUpgrader _sqliteDatabaseSchemaUpgrader = sqliteDatabaseSchemaUpgrader;
    private readonly ILogger<AppStartupWorkService> _logger = logger;

    public void Execute()
    {
        RunStartupStep("sqlite-bootstrap", _sqliteDatabaseBootstrapper.EnsureDbExists);
        RunStartupStep(
            "sqlite-schema-upgrade",
            () => _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(SqliteDatabaseBootstrapper.DatabasePath));
        RunStartupStep("automated-backup-scheduler-sync", _automatedBackupConfigurationService.SynchronizeScheduler);
        RunStartupStep("reminder-scheduler-sync", _reminderConfigurationService.SynchronizeScheduler);
    }

    #region Private Helper Methods

    private void RunStartupStep(string startupStep, Action action)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            action();

            _logger.LogInformation(
                "App startup step {StartupStep} completed in {ElapsedMilliseconds} ms.",
                startupStep,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "App startup step {StartupStep} failed after {ElapsedMilliseconds} ms.",
                startupStep,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    #endregion
}
