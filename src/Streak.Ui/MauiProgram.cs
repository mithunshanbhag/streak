namespace Streak.Ui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var app = MauiApp.CreateBuilder()
            .ConfigureApp()
            .ConfigureServices()
            .Build();

        //app.Services.GetRequiredService<SqliteDatabaseBootstrapper>()
        //    .EnsureDbExistsAsync()
        //    .GetAwaiter()
        //    .GetResult();

        return app;
    }
}