namespace Streak.Ui.Misc.Utilities;

internal static class CheckinProofPathUtility
{
    internal static bool TryNormalizeRelativeProofPath(string? relativeProofPath, out string normalizedRelativeProofPath)
    {
        normalizedRelativeProofPath = string.Empty;

        if (string.IsNullOrWhiteSpace(relativeProofPath))
            return false;

        var pathSegments = relativeProofPath
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (pathSegments.Length == 0)
            return false;

        if (pathSegments.Any(pathSegment => pathSegment is "." or ".."))
            return false;

        normalizedRelativeProofPath = string.Join('/', pathSegments);
        return true;
    }

    internal static string NormalizeRelativeProofPath(string relativeProofPath)
    {
        if (!TryNormalizeRelativeProofPath(relativeProofPath, out var normalizedRelativeProofPath))
            throw new InvalidOperationException("The uploaded picture proof path is invalid.");

        return normalizedRelativeProofPath;
    }

    internal static string GetDirectoryRelativePath(string normalizedRelativeProofPath)
    {
        var separatorIndex = normalizedRelativeProofPath.LastIndexOf('/');
        return separatorIndex < 0
            ? string.Empty
            : normalizedRelativeProofPath[..separatorIndex];
    }

    internal static string GetFileName(string normalizedRelativeProofPath)
    {
        var separatorIndex = normalizedRelativeProofPath.LastIndexOf('/');
        return separatorIndex < 0
            ? normalizedRelativeProofPath
            : normalizedRelativeProofPath[(separatorIndex + 1)..];
    }

    internal static string GetAbsolutePath(string rootDirectoryPath, string normalizedRelativeProofPath)
    {
        var pathSegments = normalizedRelativeProofPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        return Path.Combine([.. new[] { rootDirectoryPath }, .. pathSegments]);
    }
}
