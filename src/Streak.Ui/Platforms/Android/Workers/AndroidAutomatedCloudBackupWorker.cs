using Android.Content;
using AndroidX.Work;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android.Workers;

public sealed class AndroidAutomatedCloudBackupWorker(Context context, WorkerParameters workerParameters)
    : Worker(context, workerParameters)
{
    public override ListenableWorker.Result DoWork()
    {
        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var logger = services.GetRequiredService<ILogger<AndroidAutomatedCloudBackupWorker>>();
        var automatedBackupCompletionNotifier = services.GetRequiredService<IAutomatedBackupCompletionNotifier>();
        var automatedBackupRunService = services.GetRequiredService<IAutomatedBackupRunService>();

        logger.LogInformation(
            "Nightly automated OneDrive backup worker starting. Execution mode: {ExecutionMode}. Attempt: {AttemptNumber}.",
            AutomatedBackupConstants.CloudWorkerExecutionMode,
            RunAttemptCount + 1);

        try
        {
            var runResult = automatedBackupRunService.ExecuteEnabledCloudBackupAsync().GetAwaiter().GetResult();
            if (!runResult.CloudEnabled)
            {
                logger.LogInformation(
                    "Nightly automated OneDrive backup worker skipped because OneDrive automated backups are disabled.");
                return ListenableWorker.Result.InvokeSuccess()!;
            }

            if (runResult.CloudSucceeded)
            {
                logger.LogInformation("Nightly automated OneDrive backup worker completed successfully.");
                return ListenableWorker.Result.InvokeSuccess()!;
            }

            if (runResult.CloudFailureKind == OneDriveBackupFailureKind.NetworkUnavailable)
            {
                logger.LogWarning(
                    "Nightly automated OneDrive backup worker will retry after a transient network failure. Attempt: {AttemptNumber}.",
                    RunAttemptCount + 1);
                return ListenableWorker.Result.InvokeRetry()!;
            }

            automatedBackupCompletionNotifier.NotifyFailed(runResult);
            logger.LogWarning(
                "Nightly automated OneDrive backup worker completed with a terminal failure. Cloud failure kind: {CloudFailureKind}. Failure type: {FailureType}. Message: {FailureMessage}.",
                runResult.CloudFailureKind,
                runResult.CloudFailure?.GetType().FullName,
                runResult.CloudFailure?.Message);
            return ListenableWorker.Result.InvokeFailure()!;
        }
        catch (Exception exception)
        {
            automatedBackupCompletionNotifier.NotifyFailed(new AutomatedBackupRunResult
            {
                CloudEnabled = true,
                CloudSucceeded = false,
                CloudFailureKind = OneDriveBackupFailureKind.Unknown,
                CloudFailure = exception
            });
            logger.LogError(exception, "Nightly automated OneDrive backup worker failed unexpectedly.");
            return ListenableWorker.Result.InvokeFailure()!;
        }
    }
}
