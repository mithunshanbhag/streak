namespace Streak.Ui.Misc.Utilities;

public static class DiagnosticsStoragePathHelper
{
    public static string GetDiagnosticsDirectoryPath(string appDataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appDataDirectory);

        return Path.Combine(appDataDirectory, DiagnosticsConstants.DiagnosticsDirectoryName);
    }

    public static string GetDiagnosticsLogFilePath(string appDataDirectory)
    {
        return Path.Combine(
            GetDiagnosticsDirectoryPath(appDataDirectory),
            DiagnosticsConstants.StructuredLogFileName);
    }
}
