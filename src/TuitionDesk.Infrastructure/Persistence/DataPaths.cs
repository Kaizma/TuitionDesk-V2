namespace TuitionDesk.Infrastructure.Persistence;

/// <summary>Resolves the on-disk locations used by the application.</summary>
public static class DataPaths
{
    public const string EnvironmentVariable = "TUITIONDESK_DATA_DIR";
    public const string AppFolderName = "TuitionDesk";

    public static string ResolveRoot()
    {
        var configured = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);
    }

    public static string DatabasePath(string root) => Path.Combine(root, "data", "tuitiondesk.db");

    public static string BackupDirectory(string root) => Path.Combine(root, "data", "backups");

    public static string LogDirectory(string root) => Path.Combine(root, "logs");

    public static string WebViewDataDirectory(string root) => Path.Combine(root, "webview2");
}
