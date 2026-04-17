using System.IO.Compression;
using System.Text.Json;

namespace Streak.Ui.UnitTests.Services;

public sealed class DiagnosticsExportServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ExportDiagnosticsAsync_ShouldCreateBundleWithManifestAndStructuredLog()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        var diagnosticsLogPath = Path.Combine(diagnosticsDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        await File.WriteAllTextAsync(diagnosticsLogPath, """{"@t":"2026-04-17T00:00:00.0000000Z","@mt":"Sample log"}""");

        var fileSaverMock = new Mock<IDiagnosticsExportFileSaver>();
        string? savedBundlePath = null;
        string? inspectedBundlePath = null;

        fileSaverMock
            .Setup(x => x.SaveBundleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((filePath, _) =>
            {
                savedBundlePath = filePath;
                inspectedBundlePath = Path.Combine(exportDirectory.Path, "inspected-diagnostics.zip");
                File.Copy(filePath, inspectedBundlePath, true);

                return Task.FromResult(DiagnosticsExportResult.Saved);
            });

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            diagnosticsLogPath,
            fileSaverMock.Object);

        var exportResult = await sut.ExportDiagnosticsAsync();

        exportResult.Should().Be(DiagnosticsExportResult.Saved);
        savedBundlePath.Should().NotBeNull();
        Path.GetFileName(savedBundlePath!).Should().MatchRegex("^streak-diagnostics-[0-9]{8}-[0-9]{6}\\.zip$");
        File.Exists(savedBundlePath!).Should().BeFalse();
        File.Exists(inspectedBundlePath!).Should().BeTrue();

        using var zipArchive = ZipFile.OpenRead(inspectedBundlePath!);
        zipArchive.Entries.Select(x => x.FullName).Should().Contain(["logs/streak-diagnostics.log", "manifest.json"]);

        var logEntry = zipArchive.GetEntry("logs/streak-diagnostics.log");
        logEntry.Should().NotBeNull();
        using (var logReader = new StreamReader(logEntry!.Open()))
            (await logReader.ReadToEndAsync()).Should().Contain("Sample log");

        var manifestEntry = zipArchive.GetEntry("manifest.json");
        manifestEntry.Should().NotBeNull();

        using var manifestReader = new StreamReader(manifestEntry!.Open());
        var manifest = JsonDocument.Parse(await manifestReader.ReadToEndAsync());

        manifest.RootElement.GetProperty("platform").GetString().Should().NotBeNullOrWhiteSpace();
        manifest.RootElement.GetProperty("osVersion").GetString().Should().NotBeNullOrWhiteSpace();
        manifest.RootElement.GetProperty("structuredLogDirectory").GetString().Should().Be(diagnosticsDirectory.Path);
        manifest.RootElement.GetProperty("exportedAtUtc").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExportDiagnosticsAsync_ShouldStillSucceed_WhenNoStructuredLogFileExists()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        var diagnosticsLogPath = Path.Combine(diagnosticsDirectory.Path, DiagnosticsConstants.StructuredLogFileName);

        var fileSaverMock = new Mock<IDiagnosticsExportFileSaver>();
        string? inspectedBundlePath = null;

        fileSaverMock
            .Setup(x => x.SaveBundleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((filePath, _) =>
            {
                inspectedBundlePath = Path.Combine(exportDirectory.Path, "inspected-empty-diagnostics.zip");
                File.Copy(filePath, inspectedBundlePath, true);

                return Task.FromResult(DiagnosticsExportResult.Saved);
            });

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            diagnosticsLogPath,
            fileSaverMock.Object);

        var exportResult = await sut.ExportDiagnosticsAsync();

        exportResult.Should().Be(DiagnosticsExportResult.Saved);
        File.Exists(inspectedBundlePath!).Should().BeTrue();

        using var zipArchive = ZipFile.OpenRead(inspectedBundlePath!);
        zipArchive.Entries.Select(x => x.FullName).Should().BeEquivalentTo(["manifest.json"]);
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task ExportDiagnosticsAsync_ShouldReturnCancelled_WhenSaveIsCancelled()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        var diagnosticsLogPath = Path.Combine(diagnosticsDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        await File.WriteAllTextAsync(diagnosticsLogPath, "sample");

        var fileSaverMock = new Mock<IDiagnosticsExportFileSaver>();
        fileSaverMock
            .Setup(x => x.SaveBundleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DiagnosticsExportResult.Cancelled);

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            diagnosticsLogPath,
            fileSaverMock.Object);

        var exportResult = await sut.ExportDiagnosticsAsync();

        exportResult.Should().Be(DiagnosticsExportResult.Cancelled);
        Directory.GetFiles(exportDirectory.Path).Should().BeEmpty();
    }

    [Fact]
    public async Task ExportDiagnosticsAsync_ShouldPropagateSaveFailures()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        var diagnosticsLogPath = Path.Combine(diagnosticsDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        await File.WriteAllTextAsync(diagnosticsLogPath, "sample");

        var fileSaverMock = new Mock<IDiagnosticsExportFileSaver>();
        fileSaverMock
            .Setup(x => x.SaveBundleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed."));

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            diagnosticsLogPath,
            fileSaverMock.Object);

        var act = () => sut.ExportDiagnosticsAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Save failed.");

        Directory.GetFiles(exportDirectory.Path).Should().BeEmpty();
    }

    #endregion

    #region Private Helper Methods

    private static DiagnosticsExportService CreateSut(
        string exportDirectoryPath,
        string diagnosticsDirectoryPath,
        string diagnosticsLogFilePath,
        IDiagnosticsExportFileSaver fileSaver)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectoryPath);
        appStoragePathServiceMock.SetupGet(x => x.DiagnosticsDirectoryPath).Returns(diagnosticsDirectoryPath);
        appStoragePathServiceMock.SetupGet(x => x.DiagnosticsLogFilePath).Returns(diagnosticsLogFilePath);

        var loggerMock = new Mock<ILogger<DiagnosticsExportService>>();

        return new DiagnosticsExportService(
            appStoragePathServiceMock.Object,
            fileSaver,
            TimeProvider.System,
            loggerMock.Object);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"streak-ui-tests-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
        }
    }

    #endregion
}
