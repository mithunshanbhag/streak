namespace Streak.Ui.UnitTests.Services;

public sealed class BackupArchiveFactoryTests
{
    #region Positive and boundary tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ConcurrentBackups_ShouldOwnIndependentFiles(int failureMode)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"streak-concurrency-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var database = Path.Combine(directory, "source.db");
            using (var connection = new SqliteConnection($"Data Source={database};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Checkins (ProofImageUri TEXT); INSERT INTO Checkins VALUES ('Habit-1/2026/04/2026-04-21/proof.jpg');";
                command.ExecuteNonQuery();
            }
            var paths = new Mock<IAppStoragePathService>();
            paths.SetupGet(x => x.DatabasePath).Returns(database);
            paths.SetupGet(x => x.ExportDirectoryPath).Returns(directory);
            var arrived = 0;
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Mock<ICheckinProofFileStore> CreateStore(bool fail)
            {
                var store = new Mock<ICheckinProofFileStore>();
                store.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns(async () =>
                    {
                        if (Interlocked.Increment(ref arrived) == 2)
                            gate.TrySetResult();
                        await gate.Task.WaitAsync(TimeSpan.FromSeconds(10));
                        return true;
                    });
                store.Setup(x => x.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns(() => fail ? Task.FromException<Stream>(failureMode == 2
                        ? new OperationCanceledException() : new IOException("Injected proof read failure"))
                        : Task.FromResult<Stream>(new MemoryStream([1, 2, 3])));
                return store;
            }
            var firstFactory = new BackupArchiveFactory(paths.Object, CreateStore(failureMode != 0).Object, new FrozenTimeProvider());
            var secondFactory = new BackupArchiveFactory(paths.Object, CreateStore(false).Object, new FrozenTimeProvider());
            var firstTask = firstFactory.CreateAutomatedBackupAsync();
            var secondTask = secondFactory.CreateAutomatedBackupAsync();
            using var second = await secondTask;
            if (failureMode != 0)
            {
                Func<Task> act = async () => await firstTask;
                if (failureMode == 2)
                    await act.Should().ThrowAsync<OperationCanceledException>();
                else
                    await act.Should().ThrowAsync<IOException>();
            }
            else
            {
                using var first = await firstTask;
                first.WorkingFilePath.Should().NotBe(second.WorkingFilePath);
                ValidateArchive(first.WorkingFilePath, directory);
                first.Dispose();
            }
            File.Exists(second.WorkingFilePath).Should().BeTrue();
            ValidateArchive(second.WorkingFilePath, directory);
            Directory.GetFiles(directory, "streak-auto-*.zip").Should().ContainSingle().Which.Should().Be(second.WorkingFilePath);
            Directory.GetFiles(directory, "*-database.db*").Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ManualPaths_ShouldBeUniqueWithFrozenClock()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"streak-paths-{Guid.NewGuid():N}");
        try
        {
            var first = DataBackupArchiveUtility.CreateBackupFilePath(directory, new FrozenTimeProvider());
            var second = DataBackupArchiveUtility.CreateBackupFilePath(directory, new FrozenTimeProvider());
            first.Should().NotBe(second);
            Path.GetFileName(first).Should().MatchRegex("^streak-data-backup-[0-9]{8}-[0-9]{6}-[0-9a-f]{32}\\.zip$");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    #endregion

    #region Helpers

    private static void ValidateArchive(string path, string directory)
    {
        using var zip = ZipFile.OpenRead(path);
        using var proof = zip.GetEntry("CheckinProofs/Habit-1/2026/04/2026-04-21/proof.jpg")!.Open();
        using var buffer = new MemoryStream();
        proof.CopyTo(buffer);
        buffer.ToArray().Should().Equal(1, 2, 3);
        var snapshot = Path.Combine(directory, $"verify-{Guid.NewGuid():N}.db");
        zip.GetEntry("streak.db")!.ExtractToFile(snapshot);
        using var connection = new SqliteConnection($"Data Source={snapshot};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Checkins";
        Convert.ToInt32(command.ExecuteScalar()).Should().Be(1);
    }

    private sealed class FrozenTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);
    }

    #endregion
}
