namespace Streak.Ui.Services.Interfaces;

public interface IDatabaseExportService
{
    Task<DatabaseExportResult> ExportDatabaseAsync(CancellationToken cancellationToken = default);
}