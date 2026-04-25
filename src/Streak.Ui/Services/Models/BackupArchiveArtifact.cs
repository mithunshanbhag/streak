namespace Streak.Ui.Services.Models;

public sealed class BackupArchiveArtifact : IDisposable
{
    private bool _disposed;

    public required string WorkingFilePath { get; init; }

    public required IReadOnlyList<string> UnavailableReferencedProofPaths { get; init; }

    public string FileName => Path.GetFileName(WorkingFilePath);

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (File.Exists(WorkingFilePath))
            File.Delete(WorkingFilePath);
    }
}
