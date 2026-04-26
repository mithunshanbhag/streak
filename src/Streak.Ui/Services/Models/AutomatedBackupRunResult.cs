namespace Streak.Ui.Services.Models;

public sealed record AutomatedBackupRunResult
{
    public bool LocalEnabled { get; init; }

    public bool LocalSucceeded { get; init; }

    public SavedFileLocation? LocalSavedLocation { get; init; }

    public bool CloudEnabled { get; init; }

    public bool CloudSucceeded { get; init; }

    public bool HasAnySuccess => LocalSucceeded || CloudSucceeded;

    public bool HasAnyFailure =>
        (LocalEnabled && !LocalSucceeded)
        || (CloudEnabled && !CloudSucceeded);
}
