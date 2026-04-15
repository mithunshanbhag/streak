namespace Streak.Ui;

public partial class App : Application
{
    private readonly IAutomatedBackupConfigurationService _automatedBackupConfigurationService;
    private readonly SqliteDatabaseBootstrapper _sqliteDatabaseBootstrapper;
    private readonly SqliteDatabaseSchemaUpgrader _sqliteDatabaseSchemaUpgrader;

    public App(
        IAutomatedBackupConfigurationService automatedBackupConfigurationService,
        SqliteDatabaseBootstrapper sqliteDatabaseBootstrapper,
        SqliteDatabaseSchemaUpgrader sqliteDatabaseSchemaUpgrader)
    {
        _automatedBackupConfigurationService = automatedBackupConfigurationService;
        _sqliteDatabaseBootstrapper = sqliteDatabaseBootstrapper;
        _sqliteDatabaseSchemaUpgrader = sqliteDatabaseSchemaUpgrader;

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _sqliteDatabaseBootstrapper.EnsureDbExists();
        _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(SqliteDatabaseBootstrapper.DatabasePath);
        _automatedBackupConfigurationService.SynchronizeScheduler();

        return new Window(new MainPage()) { Title = "Streak.Ui" };
    }
}
