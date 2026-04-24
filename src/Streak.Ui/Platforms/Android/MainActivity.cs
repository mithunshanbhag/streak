using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;
using Microsoft.Identity.Client;

namespace Streak.Ui.Platforms.Android;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
                           | ConfigChanges.Orientation
                           | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout
                           | ConfigChanges.SmallestScreenSize
                           | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        StartupTiming.Mark("android-main-activity-on-create-start");

        base.OnCreate(savedInstanceState);
        AndroidActivityTracker.SetCurrent(this);

        StartupTiming.Mark("android-main-activity-base-on-create-completed");

        AndroidBackupNotificationChannelRegistrar.EnsureCreated();
        AndroidReminderNotificationChannelRegistrar.EnsureCreated();

        StartupTiming.Mark("android-main-activity-notification-channels-completed");

        // Keep the MAUI content inside Android's safe drawing area so the
        // Blazor app bar and bottom CTA stay clear of system bars.
        WindowCompat.SetDecorFitsSystemWindows(Window!, true);

        StartupTiming.Mark("android-main-activity-on-create-completed");
    }

    protected override void OnResume()
    {
        base.OnResume();
        AndroidActivityTracker.SetCurrent(this);
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }
}
