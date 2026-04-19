namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupCompletionNotifier
{
    /// <summary>
    ///     Raises any platform-native feedback needed after a nightly automated backup completes.
    /// </summary>
    /// <param name="savedFileLocation">The saved backup file and parent folder details.</param>
    void NotifyCompleted(SavedFileLocation savedFileLocation);
}
