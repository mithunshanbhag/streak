namespace Streak.Ui.UnitTests.Services;

public sealed class ManualBackupCompletionNotifierTests
{

    #region Positive tests

    [Fact]
    public void NotifyCompleted_ShouldShowSuccessSnackbarWithOpenFolderAction_WhenManualExportSucceeds()
    {
        var snackbarMock = new Mock<ISnackbar>();
        var backupFolderOpenerMock = new Mock<IBackupFolderOpener>();
        backupFolderOpenerMock
            .Setup(x => x.CanOpenFolder(BackupFolderKind.ManualExport, It.IsAny<SavedFileLocation>()))
            .Returns(true);

        Action<SnackbarOptions>? configureAction = null;
        snackbarMock
            .Setup(x => x.Add(
                It.IsAny<string>(),
                It.IsAny<Severity>(),
                It.IsAny<Action<SnackbarOptions>>(),
                It.IsAny<string>()))
            .Callback<string, Severity, Action<SnackbarOptions>, string>((_, _, configure, _) => { configureAction = configure; })
            .Returns((Snackbar?)null);

        var sut = new ManualBackupCompletionNotifier(snackbarMock.Object, backupFolderOpenerMock.Object);

        sut.NotifyCompleted(CreateSavedExportResult());

        snackbarMock.Verify(
            x => x.Add(
                "Backup saved to Downloads/Streak/Backups/Manual.",
                Severity.Success,
                It.IsAny<Action<SnackbarOptions>>(),
                It.Is<string>(key => key.Contains("manual-backup:", StringComparison.Ordinal))),
            Times.Once);

        configureAction.Should().NotBeNull();
    }

    #endregion

    #region Negative tests

    [Fact]
    public void NotifyCompleted_ShouldThrow_WhenExportWasCancelled()
    {
        var snackbarMock = new Mock<ISnackbar>();
        var backupFolderOpenerMock = new Mock<IBackupFolderOpener>();
        var sut = new ManualBackupCompletionNotifier(snackbarMock.Object, backupFolderOpenerMock.Object);

        var act = () => sut.NotifyCompleted(DatabaseExportResult.Cancelled);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*saved backup location*");

        snackbarMock.Verify(
            x => x.Add(
                It.IsAny<string>(),
                It.IsAny<Severity>(),
                It.IsAny<Action<SnackbarOptions>>(),
                It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Private Helper Methods

    private static DatabaseExportResult CreateSavedExportResult()
    {
        return DatabaseExportResult.Saved(new SavedFileLocation
        {
            SavedFileDisplayPath = "Downloads/Streak/Backups/Manual/streak-backup-20260420-004200.db",
            ParentFolderDisplayPath = StreakExportStorageConstants.ManualBackupsDisplayDirectoryPath
        });
    }

    #endregion
}
