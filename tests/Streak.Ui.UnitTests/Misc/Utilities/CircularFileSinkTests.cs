using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Parsing;

namespace Streak.Ui.UnitTests.Misc.Utilities;

public sealed class CircularFileSinkTests
{
    private static readonly MessageTemplateParser MessageTemplateParser = new();

    #region Positive tests

    [Fact]
    public void Emit_ShouldCreateStructuredLogFile_WhenFirstEventIsWritten()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var logFilePath = Path.Combine(temporaryDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        var sut = new CircularFileSink(logFilePath, new CompactJsonFormatter(), 512);

        sut.Emit(CreateLogEvent("First structured event"));

        File.Exists(logFilePath).Should().BeTrue();
        File.ReadAllText(logFilePath).Should().Contain("First structured event");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void Emit_ShouldRetainNewestEvents_WhenFileExceedsConfiguredLimit()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var logFilePath = Path.Combine(temporaryDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        var sut = new CircularFileSink(logFilePath, new CompactJsonFormatter(), 220);

        sut.Emit(CreateLogEvent("Event 1 1111111111111111111111111111111111111111"));
        sut.Emit(CreateLogEvent("Event 2 2222222222222222222222222222222222222222"));
        sut.Emit(CreateLogEvent("Event 3 3333333333333333333333333333333333333333"));

        var fileInfo = new FileInfo(logFilePath);
        var content = File.ReadAllText(logFilePath);

        fileInfo.Length.Should().BeLessThanOrEqualTo(220);
        content.Should().NotContain("Event 1 1111111111111111111111111111111111111111");
        content.Should().Contain("Event 2 2222222222222222222222222222222222222222");
        content.Should().Contain("Event 3 3333333333333333333333333333333333333333");
    }

    #endregion

    #region Private Helper Methods

    private static LogEvent CreateLogEvent(string messageTemplate)
    {
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            MessageTemplateParser.Parse(messageTemplate),
            []);
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
