namespace Streak.Ui.Services.Interfaces;

public interface IOneDriveAuthService
{
    /// <summary>
    ///     Raised when the current OneDrive authentication state changes.
    /// </summary>
    event EventHandler<OneDriveAuthState>? AuthStateChanged;

    /// <summary>
    ///     Gets the current OneDrive authentication state for the current platform and build.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<OneDriveAuthState> GetAuthStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts the interactive OneDrive sign-in flow.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     A result describing whether sign-in completed or the user cancelled it.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when OneDrive sign-in is not configured for the current build.
    /// </exception>
    /// <exception cref="NotSupportedException">
    ///     Thrown when OneDrive sign-in is not supported on the current platform.
    /// </exception>
    Task<OneDriveConnectResult> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clears the app's local OneDrive sign-in state for the current user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
