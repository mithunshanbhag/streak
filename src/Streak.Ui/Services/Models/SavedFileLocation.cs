namespace Streak.Ui.Services.Models;

public sealed class SavedFileLocation
{
    public required string SavedFileDisplayPath { get; init; }

    public required string ParentFolderDisplayPath { get; init; }

    public string? SavedFilePath { get; init; }
}
