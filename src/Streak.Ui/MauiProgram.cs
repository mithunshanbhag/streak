using Streak.Ui.Misc.ExtensionMethods;
using Streak.Ui.Misc.Startup;

namespace Streak.Ui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var app = MauiApp.CreateBuilder()
            .ConfigureApp()
            .ConfigureServices()
            .Build();

        app.Services.GetRequiredService<SqliteDatabaseBootstrapper>()
            .InitializeAsync()
            .GetAwaiter()
            .GetResult();

        return app;
    }
}
