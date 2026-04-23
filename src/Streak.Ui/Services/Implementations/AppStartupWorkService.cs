namespace Streak.Ui.Services.Implementations;

public sealed class AppStartupWorkService(
    IAutomatedBackupConfigurationService automatedBackupConfigurationService,
    IReminderConfigurationService reminderConfigurationService,
    SqliteDatabaseBootstrapper sqliteDatabaseBootstrapper,
    SqliteDatabaseSchemaUpgrader sqliteDatabaseSchemaUpgrader)
    : IAppStartupWorkService
{
    private readonly IAutomatedBackupConfigurationService _automatedBackupConfigurationService = automatedBackupConfigurationService;
    private readonly IReminderConfigurationService _reminderConfigurationService = reminderConfigurationService;
    private readonly SqliteDatabaseBootstrapper _sqliteDatabaseBootstrapper = sqliteDatabaseBootstrapper;
    private readonly SqliteDatabaseSchemaUpgrader _sqliteDatabaseSchemaUpgrader = sqliteDatabaseSchemaUpgrader;

    public void Execute()
    {
        _sqliteDatabaseBootstrapper.EnsureDbExists();
        _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(SqliteDatabaseBootstrapper.DatabasePath);
        _automatedBackupConfigurationService.SynchronizeScheduler();
        _reminderConfigurationService.SynchronizeScheduler();
    }
}
