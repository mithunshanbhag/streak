namespace Streak.Ui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp.CreateBuilder()
            .ConfigureApp()
            .ConfigureServices()
            .Build();
    }
}
