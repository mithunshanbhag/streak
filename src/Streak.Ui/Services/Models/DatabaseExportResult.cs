namespace Streak.Ui.Services.Models;

public sealed class DatabaseExportResult
{
    private DatabaseExportResult(DatabaseExportStatus status, SavedFileLocation? savedFileLocation)
    {
        Status = status;
        SavedFileLocation = savedFileLocation;
    }

    public DatabaseExportStatus Status { get; }

    public SavedFileLocation? SavedFileLocation { get; }

    public static DatabaseExportResult Cancelled { get; } = new(DatabaseExportStatus.Cancelled, null);

    public static DatabaseExportResult Saved(SavedFileLocation savedFileLocation)
    {
        ArgumentNullException.ThrowIfNull(savedFileLocation);
        return new DatabaseExportResult(DatabaseExportStatus.Saved, savedFileLocation);
    }
}
