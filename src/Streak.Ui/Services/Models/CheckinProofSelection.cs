namespace Streak.Ui.Services.Models;

public sealed class CheckinProofSelection
{
    public required string DisplayName { get; init; }

    public required string FileExtension { get; init; }

    public required string PreviewDataUrl { get; init; }

    public required string SourceDescription { get; init; }

    public required string ModifiedOn { get; init; }

    public required byte[] FileBytes { get; init; }

    public CheckinProofSource Source { get; init; }

    public long FileSizeBytes => FileBytes.LongLength;
}
