using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Infrastructure.Classes;
using TuitionDesk.Infrastructure.Enrollments;
using TuitionDesk.Infrastructure.Persistence;
using TuitionDesk.Infrastructure.Students;
using TuitionDesk.Infrastructure.Time;

namespace TuitionDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dataDirectory)
    {
        var root = Path.GetFullPath(dataDirectory);
        var databasePath = DataPaths.DatabasePath(root);

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        Directory.CreateDirectory(DataPaths.BackupDirectory(root));
        Directory.CreateDirectory(DataPaths.LogDirectory(root));
        Directory.CreateDirectory(DataPaths.WebViewDataDirectory(root));

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true,
            DefaultTimeout = 5,
            Pooling = true,
        }.ToString();

        services.AddSingleton(new DatabaseOptions(root, databasePath, connectionString));
        services.AddSingleton<SqlitePragmaInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options
                .UseSqlite(connectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<SqlitePragmaInterceptor>());
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentQueries, StudentQueries>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IClassQueries, ClassQueries>();
        services.AddScoped<ILookupQueries, LookupQueries>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICounterService, CounterService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<DatabaseBackupService>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
