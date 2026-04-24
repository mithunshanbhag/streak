namespace Streak.Ui.Services.Interfaces;

public interface IOneDriveAuthStateStore
{
    /// <summary>
    ///     Gets the last known connected OneDrive account username for the current app install.
    /// </summary>
    /// <returns>
    ///     The last known connected account username, or <see langword="null" /> when no connected account has been recorded.
    /// </returns>
    string? GetLastKnownAccountUsername();

    /// <summary>
    ///     Saves the last known connected OneDrive account username for the current app install.
    /// </summary>
    /// <param name="accountUsername">
    ///     The connected account username to persist. Passing <see langword="null" />, empty, or whitespace clears the stored value.
    /// </param>
    void SetLastKnownAccountUsername(string? accountUsername);

    /// <summary>
    ///     Clears any persisted OneDrive account username for the current app install.
    /// </summary>
    void Clear();
}
