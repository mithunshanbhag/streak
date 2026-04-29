namespace Streak.Ui.UnitTests.Services;

public sealed class CheckinProofServiceTests
{
    #region Positive tests

    [Fact]
    public async Task PickPhotoAsync_ShouldReturnPreviewReadySelection_WhenPickerReturnsImage()
    {
        using var proofDirectory = new TemporaryDirectory();
        using var sourceDirectory = new TemporaryDirectory();
        var sourcePath = Path.Combine(sourceDirectory.Path, "proof.png");
        await File.WriteAllBytesAsync(sourcePath, [1, 2, 3, 4]);

        var pickerService = new FakeCheckinProofMediaPickerService(
            selectedPhoto: new FileResult(sourcePath),
            supportsCameraCapture: true);
        var sut = new CheckinProofService(
            pickerService,
            new FakeCameraPermissionService(),
            CreateProofFileStore(proofDirectory.Path));

        var result = await sut.PickPhotoAsync();

        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("proof.png");
        result.FileExtension.Should().Be(".png");
        result.Source.Should().Be(CheckinProofSource.Gallery);
        result.SourceDescription.Should().Be("Gallery");
        result.PreviewDataUrl.Should().StartWith("data:image/png;base64,");
    }

    [Fact]
    public async Task PersistAsync_ShouldSaveProofIntoHabitDateFolder_AndReturnRelativeMetadata()
    {
        using var proofDirectory = new TemporaryDirectory();
        var pickerService = new FakeCheckinProofMediaPickerService();
        var sut = new CheckinProofService(
            pickerService,
            new FakeCameraPermissionService(),
            CreateProofFileStore(proofDirectory.Path));
        var selection = new CheckinProofSelection
        {
            DisplayName = "proof.jpg",
            FileBytes = [1, 2, 3, 4],
            FileExtension = ".jpg",
            ModifiedOn = "2026-04-21T08:30:12.0000000+05:30",
            PreviewDataUrl = "data:image/jpeg;base64,AQIDBA==",
            Source = CheckinProofSource.Gallery,
            SourceDescription = "Gallery"
        };

        var result = await sut.PersistAsync(selection, 7, "2026-04-21");

        result.ProofImageUri.Should().StartWith("Habit-7/2026/04/2026-04-21/");
        result.ProofImageDisplayName.Should().Be("proof.jpg");
        result.ProofImageSizeBytes.Should().Be(4);

        var absolutePath = Path.Combine(
            proofDirectory.Path,
            result.ProofImageUri.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(absolutePath).Should().BeTrue();
        (await File.ReadAllBytesAsync(absolutePath)).Should().Equal(selection.FileBytes);
    }

    [Fact]
    public async Task DeleteIfExistsAsync_ShouldDeletePersistedProof_WhenFileExists()
    {
        using var proofDirectory = new TemporaryDirectory();
        var relativePath = "Habit-7/2026/04/2026-04-21/proof.jpg";
        var absolutePath = Path.Combine(
            proofDirectory.Path,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await File.WriteAllBytesAsync(absolutePath, [1, 2, 3, 4]);

        var pickerService = new FakeCheckinProofMediaPickerService();
        var sut = new CheckinProofService(
            pickerService,
            new FakeCameraPermissionService(),
            CreateProofFileStore(proofDirectory.Path));

        await sut.DeleteIfExistsAsync(relativePath);

        File.Exists(absolutePath).Should().BeFalse();
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task PickPhotoAsync_ShouldThrowInvalidOperationException_WhenSelectedFileExceedsMaximumSize()
    {
        using var proofDirectory = new TemporaryDirectory();
        using var sourceDirectory = new TemporaryDirectory();
        var sourcePath = Path.Combine(sourceDirectory.Path, "proof.jpg");
        await File.WriteAllBytesAsync(sourcePath, new byte[CoreConstants.CheckinProofMaxSizeBytes + 1]);

        var pickerService = new FakeCheckinProofMediaPickerService(selectedPhoto: new FileResult(sourcePath));
        var sut = new CheckinProofService(
            pickerService,
            new FakeCameraPermissionService(),
            CreateProofFileStore(proofDirectory.Path));

        var act = () => sut.PickPhotoAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Selected picture proof must be 5 MB or smaller.");
    }

    [Fact]
    public async Task CapturePhotoAsync_ShouldThrowInvalidOperationException_WhenCameraPermissionIsDenied()
    {
        using var proofDirectory = new TemporaryDirectory();
        var pickerService = new FakeCheckinProofMediaPickerService(supportsCameraCapture: true);
        var cameraPermissionService = new FakeCameraPermissionService(isGranted: false);
        var sut = new CheckinProofService(
            pickerService,
            cameraPermissionService,
            CreateProofFileStore(proofDirectory.Path));

        var act = () => sut.CapturePhotoAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Camera permission is required to take a picture. You can continue without a photo or choose Gallery instead.");
        pickerService.CapturePhotoCallCount.Should().Be(0);
    }

    #endregion

    #region Private helper methods

    private static ICheckinProofFileStore CreateProofFileStore(string proofDirectoryPath)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(proofDirectoryPath);
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(Path.Combine(proofDirectoryPath, "streak.db"));
        appStoragePathServiceMock.SetupGet(x => x.DiagnosticsDirectoryPath).Returns(Path.Combine(proofDirectoryPath, "Diagnostics"));
        appStoragePathServiceMock.SetupGet(x => x.DiagnosticsLogFilePath).Returns(Path.Combine(proofDirectoryPath, "Diagnostics", "log.txt"));
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(Path.Combine(proofDirectoryPath, "ExportWorking"));
        return new FileSystemCheckinProofFileStore(appStoragePathServiceMock.Object);
    }

    private sealed class FakeCheckinProofMediaPickerService(
        FileResult? selectedPhoto = null,
        bool supportsCameraCapture = false) : ICheckinProofMediaPickerService
    {
        public int CapturePhotoCallCount { get; private set; }

        public bool SupportsCameraCapture => supportsCameraCapture;

        public Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CapturePhotoCallCount++;
            return Task.FromResult(selectedPhoto);
        }

        public Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(selectedPhoto);
        }
    }

    private sealed class FakeCameraPermissionService(bool isGranted = true) : ICameraPermissionService
    {
        public Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(isGranted);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"streak-checkin-proof-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }

    #endregion
}
