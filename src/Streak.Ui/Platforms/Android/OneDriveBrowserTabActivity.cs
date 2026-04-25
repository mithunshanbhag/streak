using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Identity.Client;

namespace Streak.Ui.Platforms.Android;

[Activity(
    Name = "streak.ui.platforms.android.OneDriveBrowserTabActivity",
    Exported = true,
    LaunchMode = LaunchMode.SingleTask,
    NoHistory = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public sealed class OneDriveBrowserTabActivity : BrowserTabActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        LogCallbackIntent("created", Intent);
        base.OnCreate(savedInstanceState);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        LogCallbackIntent("received new intent", intent);
        base.OnNewIntent(intent);
    }

    private static void LogCallbackIntent(string lifecycleEvent, Intent? intent)
    {
        var logger = AndroidLoggerResolver.GetLogger<OneDriveBrowserTabActivity>();
        logger?.LogInformation(
            "OneDrive browser callback activity {LifecycleEvent}. Intent action: {IntentAction}. Has data: {HasData}. Data scheme: {DataScheme}. Data host: {DataHost}.",
            lifecycleEvent,
            intent?.Action ?? "(none)",
            intent?.Data is not null,
            intent?.Data?.Scheme ?? "(none)",
            intent?.Data?.Host ?? "(none)");
    }
}
