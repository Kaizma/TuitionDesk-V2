using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace TuitionDesk.Infrastructure.Persistence;

/// <summary>Creates consistent SQLite backups using the online backup API.</summary>
public sealed class DatabaseBackupService(
    DatabaseOptions options,
    ILogger<DatabaseBackupService> logger)
{
    private const int RetainedBackups = 10;

    public string DatabasePath => options.DatabasePath;

    public string BackupDirectory => options.BackupDirectory;

    public Task<string?> CreateAsync(string reason, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(options.DatabasePath))
        {
            return Task.FromResult<string?>(null);
        }

        Directory.CreateDirectory(options.BackupDirectory);

        var safeReason = string.Concat(reason.Select(c => char.IsLetterOrDigit(c) || c == '-' ? c : '-'));
        var fileName = $"tuitiondesk-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{safeReason}.db";
        var destinationPath = Path.Combine(options.BackupDirectory, fileName);

        using var source = new SqliteConnection(options.ConnectionString);
        source.Open();

        using var destination = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = destinationPath }.ToString());
        destination.Open();

        source.BackupDatabase(destination);
        logger.LogInformation("Database backup written to {BackupPath}", destinationPath);

        Prune();
        return Task.FromResult<string?>(destinationPath);
    }

    private void Prune()
    {
        try
        {
            var backups = new DirectoryInfo(options.BackupDirectory)
                .GetFiles("tuitiondesk-*.db")
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();

            foreach (var stale in backups.Skip(RetainedBackups))
            {
                stale.Delete();
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Could not prune old database backups.");
        }
    }
}
