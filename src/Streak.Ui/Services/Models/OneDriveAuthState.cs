namespace Streak.Ui.Services.Models;

public sealed record OneDriveAuthState
{
    public string? AccountUsername { get; init; }

    public bool IsConfigured { get; init; }

    public bool IsConnected { get; init; }

    public bool IsPlatformSupported { get; init; }

    public string StorageLocationDisplayName { get; init; } = OneDriveAuthConstants.StorageLocationDisplayName;
}
