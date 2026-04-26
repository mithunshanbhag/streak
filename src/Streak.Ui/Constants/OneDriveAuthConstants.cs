namespace Streak.Ui.Constants;

public static class OneDriveAuthConstants
{
    public const string AssemblyMetadataClientIdKey = "Streak.OneDriveClientId";

    public const string Authority = "https://login.microsoftonline.com/consumers";

    public const string RedirectUriHost = "auth";

    public const string StorageLocationDisplayName = "OneDrive app folder";

    public const string TokenCacheFileName = "streak-onedrive-msal-cache.dat";

    public const string UnconfiguredClientId = "00000000-0000-0000-0000-000000000000";

    public static readonly string[] DefaultScopes =
    [
        "Files.ReadWrite",
        "Files.ReadWrite.AppFolder",
        "User.Read",
        "offline_access"
    ];
}
