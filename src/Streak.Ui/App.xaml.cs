namespace Streak.Ui;

public partial class App : Application
{
    private readonly SqliteDatabaseBootstrapper _sqliteDatabaseBootstrapper;
    private readonly SqliteDatabaseSchemaUpgrader _sqliteDatabaseSchemaUpgrader;

    public App(
        SqliteDatabaseBootstrapper sqliteDatabaseBootstrapper,
        SqliteDatabaseSchemaUpgrader sqliteDatabaseSchemaUpgrader)
    {
        _sqliteDatabaseBootstrapper = sqliteDatabaseBootstrapper;
        _sqliteDatabaseSchemaUpgrader = sqliteDatabaseSchemaUpgrader;

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _sqliteDatabaseBootstrapper.EnsureDbExists();
        _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(SqliteDatabaseBootstrapper.DatabasePath);

        return new Window(new MainPage()) { Title = "Streak.Ui" };
    }
}