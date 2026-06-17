using Android.Content;
using AndroidX.Work;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android.Workers;

public sealed class AndroidAutomatedLocalBackupWorker(Context context, WorkerParameters workerParameters)
    : Worker(context, workerParameters)
{
    public override ListenableWorker.Result DoWork()
    {
        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var logger = services.GetRequiredService<ILogger<AndroidAutomatedLocalBackupWorker>>();
        var automatedBackupCompletionNotifier = services.GetRequiredService<IAutomatedBackupCompletionNotifier>();
        var automatedBackupRunService = services.GetRequiredService<IAutomatedBackupRunService>();

        logger.LogInformation(
            "Nightly automated local backup worker starting. Execution mode: {ExecutionMode}. Attempt: {AttemptNumber}.",
            AutomatedBackupConstants.LocalWorkerExecutionMode,
            RunAttemptCount + 1);

        try
        {
            var runResult = automatedBackupRunService.ExecuteEnabledLocalBackupAsync().GetAwaiter().GetResult();
            if (!runResult.LocalEnabled)
            {
                logger.LogInformation(
                    "Nightly automated local backup worker skipped because local automated backups are disabled.");
                return ListenableWorker.Result.InvokeSuccess()!;
            }

            if (runResult.LocalSavedLocation is not null)
            {
                automatedBackupCompletionNotifier.NotifyCompleted(runResult.LocalSavedLocation);
                logger.LogInformation(
                    "Nightly automated local backup worker completed successfully. Saved path: {LocalSavedPath}.",
                    runResult.LocalSavedLocation.SavedFileDisplayPath);
                return ListenableWorker.Result.InvokeSuccess()!;
            }

            automatedBackupCompletionNotifier.NotifyFailed(runResult);
            logger.LogWarning(
                "Nightly automated local backup worker completed with failure. Failure type: {FailureType}. Message: {FailureMessage}.",
                runResult.LocalFailure?.GetType().FullName,
                runResult.LocalFailure?.Message);
            return ListenableWorker.Result.InvokeFailure()!;
        }
        catch (Exception exception)
        {
            automatedBackupCompletionNotifier.NotifyFailed(new AutomatedBackupRunResult
            {
                LocalEnabled = true,
                LocalSucceeded = false,
                LocalFailure = exception
            });
            logger.LogError(exception, "Nightly automated local backup worker failed unexpectedly.");
            return ListenableWorker.Result.InvokeFailure()!;
        }
    }
}
