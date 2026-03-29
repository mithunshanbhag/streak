using Android.App;
using Android.Content.PM;
using Android.OS;

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
    public override void SetTheme(int resid)
    {
        // When MAUI internally transitions from the splash screen it calls
        // SetTheme(Resource.Style.Maui_MainTheme). We intercept that call and
        // redirect to AppTheme, which is a thin wrapper around Maui.MainTheme
        // and additionally opts out of Android 15+ (API 35) forced edge-to-edge
        // enforcement via values-v35/styles.xml. This keeps the splash screen
        // working normally while ensuring the main activity uses our custom theme.
        // Note: Resource.Style.Maui_MainTheme is a compile-time generated constant,
        // so any MAUI rename would surface as a build error rather than a silent failure.
        if (resid == Resource.Style.Maui_MainTheme)
        {
            base.SetTheme(Resource.Style.AppTheme);
            return;
        }

        base.SetTheme(resid);
    }
}