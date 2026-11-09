using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TuitionDesk.Infrastructure.Persistence;

/// <summary>Applies migrations safely and verifies the database file before the app starts serving.</summary>
public sealed class DatabaseInitializer(
    AppDbContext dbContext,
    DatabaseBackupService backups,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var databaseExisted = File.Exists(backups.DatabasePath);
        var pending = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pending.Count > 0)
        {
            if (databaseExisted)
            {
                await backups.CreateAsync("pre-migration", cancellationToken);
            }

            logger.LogInformation(
                "Applying {Count} migration(s): {Migrations}",
                pending.Count,
                string.Join(", ", pending));

            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        await VerifyIntegrityAsync(cancellationToken);

        if (databaseExisted)
        {
            await EnsureDailyBackupAsync(cancellationToken);
        }
    }

    private async Task EnsureDailyBackupAsync(CancellationToken cancellationToken)
    {
        var newest = Directory.Exists(backups.BackupDirectory)
            ? new DirectoryInfo(backups.BackupDirectory)
                .GetFiles("tuitiondesk-*.db")
                .OrderByDescending(file => file.CreationTimeUtc)
                .FirstOrDefault()
            : null;

        if (newest is null || newest.CreationTimeUtc < DateTime.UtcNow.AddHours(-24))
        {
            await backups.CreateAsync("daily", cancellationToken);
        }
    }

    private async Task VerifyIntegrityAsync(CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;

        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA quick_check;";
            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is not string text || !text.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The database failed its integrity check. Restore a backup before continuing.");
            }
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }
    }
}
