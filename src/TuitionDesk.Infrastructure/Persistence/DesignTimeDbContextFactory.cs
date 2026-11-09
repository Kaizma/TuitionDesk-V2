using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TuitionDesk.Infrastructure.Persistence;

/// <summary>Used by <c>dotnet ef</c>; never points at the user's real database.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var root = Environment.GetEnvironmentVariable(DataPaths.EnvironmentVariable)
            ?? Path.Combine(Path.GetTempPath(), "tuitiondesk-design");

        Directory.CreateDirectory(root);
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(root, "design.db"),
        }.ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
