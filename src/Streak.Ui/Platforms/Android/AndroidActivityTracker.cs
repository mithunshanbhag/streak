namespace Streak.Ui.Platforms.Android;

public static class AndroidActivityTracker
{
    private static readonly object SyncRoot = new();

    private static WeakReference<global::Android.App.Activity>? _currentActivity;

    public static global::Android.App.Activity GetRequiredCurrentActivity()
    {
        lock (SyncRoot)
        {
            if (_currentActivity?.TryGetTarget(out var activity) == true)
                return activity;
        }

        throw new InvalidOperationException("No current Android activity is available for OneDrive sign-in.");
    }

    public static void SetCurrent(global::Android.App.Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        lock (SyncRoot)
        {
            _currentActivity = new WeakReference<global::Android.App.Activity>(activity);
        }
    }
}
