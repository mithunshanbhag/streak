using Streak.Ui.Misc.ExtensionMethods;

namespace Streak.Ui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp() =>
        MauiApp.CreateBuilder()
            .ConfigureApp()
            .ConfigureServices()
            .Build();
}