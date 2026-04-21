namespace Streak.Ui.Misc.Utilities;

internal static class CheckinProofFileUtility
{
    internal static string NormalizeStoredFileExtension(string? fileExtension)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
            return ".jpg";

        return fileExtension.StartsWith('.')
            ? fileExtension.ToLowerInvariant()
            : $".{fileExtension.ToLowerInvariant()}";
    }

    internal static string GetMimeType(string fileExtension)
    {
        return NormalizeStoredFileExtension(fileExtension) switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }
}
