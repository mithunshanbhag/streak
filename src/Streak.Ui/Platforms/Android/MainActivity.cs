using Android.App;
using Android.Content.PM;

namespace Streak.Ui.Platforms.Android;

[Activity(
    Theme = "@style/AppTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
                           | ConfigChanges.Orientation
                           | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout
                           | ConfigChanges.SmallestScreenSize
                           | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity;