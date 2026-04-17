using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

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
        base.OnCreate(savedInstanceState);

        // Keep the MAUI content inside Android's safe drawing area so the
        // Blazor app bar and bottom CTA stay clear of system bars.
        WindowCompat.SetDecorFitsSystemWindows(Window!, true);
    }
}
