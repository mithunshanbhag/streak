using Android.Content;
using AndroidX.Work;

namespace Streak.Ui.Platforms.Android.Services;

internal static class AndroidAutomatedBackupWorkEnqueuer
{
    public static void SynchronizeQueuedBackups(
        Context context,
        bool localEnabled,
        bool cloudEnabled)
    {
        ArgumentNullException.ThrowIfNull(context);

        var workManager = WorkManager.GetInstance(context);

        if (localEnabled)
        {
            workManager.EnqueueUniqueWork(
                AutomatedBackupConstants.LocalWorkerUniqueWorkName,
                ExistingWorkPolicy.Replace!,
                BuildLocalWorkRequest());
        }
        else
        {
            workManager.CancelUniqueWork(AutomatedBackupConstants.LocalWorkerUniqueWorkName);
        }

        if (cloudEnabled)
        {
            workManager.EnqueueUniqueWork(
                AutomatedBackupConstants.CloudWorkerUniqueWorkName,
                ExistingWorkPolicy.Replace!,
                BuildCloudWorkRequest());
        }
        else
        {
            workManager.CancelUniqueWork(AutomatedBackupConstants.CloudWorkerUniqueWorkName);
        }
    }

    private static OneTimeWorkRequest BuildLocalWorkRequest()
    {
        return new OneTimeWorkRequest.Builder(typeof(Workers.AndroidAutomatedLocalBackupWorker))
            .Build();
    }

    private static OneTimeWorkRequest BuildCloudWorkRequest()
    {
        var constraints = new Constraints.Builder()
            .SetRequiredNetworkType(NetworkType.Connected!)
            .Build();

        return new OneTimeWorkRequest.Builder(typeof(Workers.AndroidAutomatedCloudBackupWorker))
            .SetConstraints(constraints)
            .SetBackoffCriteria(
                BackoffPolicy.Linear!,
                TimeSpan.FromMinutes(AutomatedBackupConstants.CloudRetryBackoffMinutes))
            .Build();
    }
}
