using System.IO;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Desktop;

/// <summary>
/// Lightweight file logging for the window bootstrap, before Serilog is configured.
/// Prefers the test/application data directory, then the executable folder.
/// </summary>
internal static class StartupDiagnostics
{
    private static readonly object Gate = new();

    public static string LogPath { get; } = ResolveLogPath();

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(
                    LogPath,
                    $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} {message}{Environment.NewLine}");
            }
        }
        catch (Exception)
        {
            // Diagnostics must never take the app down.
        }
    }

    private static string ResolveLogPath()
    {
        var configured = Environment.GetEnvironmentVariable("TUITIONDESK_DATA_DIR");
        var root = string.IsNullOrWhiteSpace(configured) ? DataPaths.ResolveRoot() : configured;
        return Path.Combine(root, "startup.log");
    }
}
