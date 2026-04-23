namespace Streak.Ui.Misc.Utilities;

internal static class CheckinProofMediaStorePathUtility
{
    internal static string NormalizeMediaStoreRelativePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        return string.Join(
            '/',
            relativePath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    internal static bool TryBuildRelativeProofPath(
        string rootRelativePath,
        string? mediaStoreRelativePath,
        string? displayName,
        out string relativeProofPath)
    {
        relativeProofPath = string.Empty;

        if (string.IsNullOrWhiteSpace(rootRelativePath) || string.IsNullOrWhiteSpace(displayName))
            return false;

        var normalizedRootRelativePath = NormalizeMediaStoreRelativePath(rootRelativePath);
        var normalizedMediaStoreRelativePath = NormalizeMediaStoreRelativePath(mediaStoreRelativePath);

        if (string.IsNullOrWhiteSpace(normalizedRootRelativePath)
            || string.IsNullOrWhiteSpace(normalizedMediaStoreRelativePath))
        {
            return false;
        }

        string candidateRelativeProofPath;
        if (string.Equals(
                normalizedMediaStoreRelativePath,
                normalizedRootRelativePath,
                StringComparison.Ordinal))
        {
            candidateRelativeProofPath = displayName;
        }
        else
        {
            var rootWithSeparator = $"{normalizedRootRelativePath}/";
            if (!normalizedMediaStoreRelativePath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
                return false;

            var nestedRelativePath = normalizedMediaStoreRelativePath[rootWithSeparator.Length..].Trim('/');
            candidateRelativeProofPath = string.IsNullOrWhiteSpace(nestedRelativePath)
                ? displayName
                : $"{nestedRelativePath}/{displayName}";
        }

        return CheckinProofPathUtility.TryNormalizeRelativeProofPath(
            candidateRelativeProofPath,
            out relativeProofPath);
    }
}
