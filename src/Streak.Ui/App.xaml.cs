namespace Streak.Ui;

public partial class App : Application
{
    private readonly SqliteDatabaseBootstrapper _sqliteDatabaseBootstrapper;

    public App(SqliteDatabaseBootstrapper sqliteDatabaseBootstrapper)
    {
        _sqliteDatabaseBootstrapper = sqliteDatabaseBootstrapper;
        
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _sqliteDatabaseBootstrapper.EnsureDbExists();

        return new Window(new MainPage()) { Title = "Streak.Ui" };
    }
}
