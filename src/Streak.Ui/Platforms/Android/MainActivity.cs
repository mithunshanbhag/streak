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
    LaunchMode = LaunchMode.SingleTop,
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
        LogIntent("created", Intent);

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
        LogIntent("resumed", Intent);
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        LogActivityResult(requestCode, resultCode, data);
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Intent = intent;
        AndroidActivityTracker.SetCurrent(this);
        LogIntent("received new intent", intent);
    }

    private static void LogIntent(string lifecycleEvent, Intent? intent)
    {
        var logger = AndroidLoggerResolver.GetLogger<MainActivity>();
        logger?.LogInformation(
            "MainActivity {LifecycleEvent}. Intent action: {IntentAction}. Has data: {HasData}. Data scheme: {DataScheme}. Data host: {DataHost}.",
            lifecycleEvent,
            intent?.Action ?? "(none)",
            intent?.Data is not null,
            AndroidLoggerResolver.GetSafeDataScheme(intent?.Data?.Scheme),
            intent?.Data?.Host ?? "(none)");
    }

    private static void LogActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        var logger = AndroidLoggerResolver.GetLogger<MainActivity>();
        logger?.LogInformation(
            "MainActivity activity result received. Request code: {RequestCode}. Result code: {ResultCode}. Has data: {HasData}. Data scheme: {DataScheme}. Data host: {DataHost}.",
            requestCode,
            resultCode,
            data?.Data is not null,
            AndroidLoggerResolver.GetSafeDataScheme(data?.Data?.Scheme),
            data?.Data?.Host ?? "(none)");
    }
}
