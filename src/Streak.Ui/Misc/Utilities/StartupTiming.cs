namespace Streak.Ui.Misc.Utilities;

public static class StartupTiming
{
    private static readonly object SyncRoot = new();
    private static readonly List<StartupTimingMark> Marks = [];

    private static bool _hasLoggedSnapshot;

    public static void Mark(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var timestamp = Stopwatch.GetTimestamp();

        lock (SyncRoot)
        {
            Marks.Add(new StartupTimingMark(name.Trim(), timestamp));
        }
    }

    public static void LogSnapshot<T>(ILogger<T> logger, string checkpoint)
    {
        ArgumentNullException.ThrowIfNull(logger);

        IReadOnlyList<StartupTimingSnapshotEntry> snapshot;

        lock (SyncRoot)
        {
            if (_hasLoggedSnapshot)
                return;

            _hasLoggedSnapshot = true;
            snapshot = CreateSnapshotCore();
        }

        foreach (var entry in snapshot)
        {
            logger.LogInformation(
                "Startup timing mark {StartupTimingMark} reached at +{ElapsedMillisecondsSinceFirstMark} ms ({DeltaMillisecondsSincePreviousMark} ms since previous mark) before checkpoint {StartupTimingCheckpoint}.",
                entry.Name,
                entry.ElapsedMillisecondsSinceFirstMark,
                entry.DeltaMillisecondsSincePreviousMark,
                checkpoint);
        }
    }

    private static IReadOnlyList<StartupTimingSnapshotEntry> CreateSnapshotCore()
    {
        if (Marks.Count == 0)
            return [];

        var firstTimestamp = Marks[0].Timestamp;
        var previousTimestamp = firstTimestamp;
        List<StartupTimingSnapshotEntry> snapshot = [];

        foreach (var mark in Marks)
        {
            var elapsed = Stopwatch.GetElapsedTime(firstTimestamp, mark.Timestamp);
            var delta = Stopwatch.GetElapsedTime(previousTimestamp, mark.Timestamp);

            snapshot.Add(new StartupTimingSnapshotEntry(
                mark.Name,
                (long)elapsed.TotalMilliseconds,
                (long)delta.TotalMilliseconds));

            previousTimestamp = mark.Timestamp;
        }

        return snapshot;
    }

    private sealed record StartupTimingMark(string Name, long Timestamp);

    private sealed record StartupTimingSnapshotEntry(
        string Name,
        long ElapsedMillisecondsSinceFirstMark,
        long DeltaMillisecondsSincePreviousMark);
}
