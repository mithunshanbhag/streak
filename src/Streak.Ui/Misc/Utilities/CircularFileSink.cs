using System.Text;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;

namespace Streak.Ui.Misc.Utilities;

public sealed class CircularFileSink : ILogEventSink
{
    private static readonly Encoding Utf8Encoding = new UTF8Encoding(false);

    private readonly string _filePath;
    private readonly long _fileSizeLimitBytes;
    private readonly ITextFormatter _textFormatter;
    private readonly object _syncRoot = new();

    public CircularFileSink(
        string filePath,
        ITextFormatter textFormatter,
        long fileSizeLimitBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(textFormatter);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSizeLimitBytes);

        _filePath = filePath;
        _textFormatter = textFormatter;
        _fileSizeLimitBytes = fileSizeLimitBytes;
    }

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        try
        {
            var logEventBytes = FormatLogEvent(logEvent);
            if (logEventBytes.Length > _fileSizeLimitBytes)
                logEventBytes = CreateOversizedEventBytes(logEventBytes.Length);

            lock (_syncRoot)
            {
                EnsureDirectoryExists();

                using var stream = new FileStream(
                    _filePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);

                var existingLength = stream.Length;
                if (existingLength + logEventBytes.Length <= _fileSizeLimitBytes)
                {
                    stream.Position = existingLength;
                    stream.Write(logEventBytes);
                    stream.Flush();
                    return;
                }

                var retainedBytes = ReadRetainedBytes(stream, logEventBytes.Length);

                stream.Position = 0;
                stream.SetLength(0);

                if (retainedBytes.Length > 0)
                    stream.Write(retainedBytes);

                stream.Write(logEventBytes);
                stream.Flush();
            }
        }
        catch (Exception exception)
        {
            SelfLog.WriteLine(
                "Failed to write diagnostics log event to {0}: {1}",
                _filePath,
                exception);
        }
    }

    private byte[] FormatLogEvent(LogEvent logEvent)
    {
        using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
        _textFormatter.Format(logEvent, stringWriter);

        var payload = stringWriter.ToString();
        if (!payload.EndsWith('\n'))
            payload += Environment.NewLine;

        return Utf8Encoding.GetBytes(payload);
    }

    private byte[] CreateOversizedEventBytes(int eventSizeBytes)
    {
        var payload =
            $$"""
              {"@t":"{{DateTimeOffset.UtcNow:O}}","@l":"Warning","@mt":"Dropped an oversized structured log event because it exceeded the single-file diagnostics cap.","EventSizeBytes":{{eventSizeBytes}},"FileSizeLimitBytes":{{_fileSizeLimitBytes}}}
              """;

        return Utf8Encoding.GetBytes(payload + Environment.NewLine);
    }

    private byte[] ReadRetainedBytes(FileStream stream, int incomingEventByteCount)
    {
        var bytesToRetain = checked((int)Math.Max(0, _fileSizeLimitBytes - incomingEventByteCount));
        if (bytesToRetain == 0 || stream.Length == 0)
            return [];

        var existingBytes = new byte[checked((int)stream.Length)];
        stream.Position = 0;
        stream.ReadExactly(existingBytes);

        if (existingBytes.Length <= bytesToRetain)
            return existingBytes;

        var cutoffIndex = existingBytes.Length - bytesToRetain;
        var retainedStartIndex = FindRetainedStartIndex(existingBytes, cutoffIndex);

        return retainedStartIndex >= existingBytes.Length
            ? []
            : existingBytes[retainedStartIndex..];
    }

    private static int FindRetainedStartIndex(byte[] existingBytes, int cutoffIndex)
    {
        if (cutoffIndex <= 0)
            return 0;

        var newlineIndex = Array.IndexOf(existingBytes, (byte)'\n', cutoffIndex);
        return newlineIndex < 0 || newlineIndex >= existingBytes.Length - 1
            ? existingBytes.Length
            : newlineIndex + 1;
    }

    private void EnsureDirectoryExists()
    {
        var directoryPath = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
            Directory.CreateDirectory(directoryPath);
    }
}
