namespace Streak.Ui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        StartupTiming.Mark("maui-program-create-maui-app-start");

        var builder = MauiApp.CreateBuilder()
            .ConfigureApp()
            .ConfigureServices();

        StartupTiming.Mark("maui-program-builder-configured");

        var app = builder.Build();

        StartupTiming.Mark("maui-program-create-maui-app-completed");

        return app;
    }
}
