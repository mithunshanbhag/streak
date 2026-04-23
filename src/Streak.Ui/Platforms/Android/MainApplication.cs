using Android.App;
using Android.Runtime;

namespace Streak.Ui.Platforms.Android;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    public override void OnCreate()
    {
        StartupTiming.Mark("android-application-on-create-start");

        base.OnCreate();

        StartupTiming.Mark("android-application-on-create-completed");
    }

    protected override MauiApp CreateMauiApp()
    {
        StartupTiming.Mark("android-application-create-maui-app-start");

        var app = MauiProgram.CreateMauiApp();

        StartupTiming.Mark("android-application-create-maui-app-completed");

        return app;
    }
}
