using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TuitionDesk.Application;
using TuitionDesk.Infrastructure;
using TuitionDesk.Infrastructure.Persistence;
using TuitionDesk.Server.Endpoints;
using TuitionDesk.Server.Http;
using TuitionDesk.Server.Middleware;

namespace TuitionDesk.Server.Hosting;

public static class ServerHost
{
    public static async Task<ServerInstance> StartAsync(
        ServerOptions options,
        CancellationToken cancellationToken = default)
    {
        var token = options.RequireToken
            ? options.Token ?? Convert.ToHexString(RandomNumberGenerator.GetBytes(32))
            : options.Token ?? string.Empty;

        var effectiveOptions = options with { Token = token };

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = options.ContentRootPath ?? Directory.GetCurrentDirectory(),
            ApplicationName = typeof(ServerHost).Assembly.GetName().Name,
            EnvironmentName = options.EnvironmentName,
        });

        builder.WebHost.UseUrls(options.Urls);
        ConfigureLogging(builder, options.DataDirectory);

        builder.Services.AddInfrastructure(options.DataDirectory);
        builder.Services.AddApplication();
        builder.Services.AddSingleton(effectiveOptions);
        builder.Services.ConfigureHttpJsonOptions(json =>
        {
            json.SerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter(
                    System.Text.Json.JsonNamingPolicy.CamelCase));
        });

        var app = builder.Build();
        ConfigurePipeline(app, effectiveOptions);

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync(cancellationToken);
        }

        await app.StartAsync(cancellationToken);

        var url = ResolveUrl(app);
        Log.Information("TuitionDesk API listening at {Url}", url);

        return new ServerInstance(app, url, token);
    }

    private static void ConfigureLogging(WebApplicationBuilder builder, string dataDirectory)
    {
        var logDirectory = DataPaths.LogDirectory(Path.GetFullPath(dataDirectory));
        Directory.CreateDirectory(logDirectory);

        builder.Host.UseSerilog((_, _, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(logDirectory, "tuitiondesk-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true));
    }

    private static void ConfigurePipeline(WebApplication app, ServerOptions options)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (options.RequireToken)
        {
            app.UseMiddleware<LoopbackTokenMiddleware>();
        }

        var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        var hasSpa = options.ServeSpa && File.Exists(Path.Combine(webRoot, "index.html"));

        if (hasSpa)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        app.MapGet("/api/health", async (AppDbContext db, CancellationToken ct) =>
        {            var canConnect = await db.Database.CanConnectAsync(ct);
            var applied = (await db.Database.GetAppliedMigrationsAsync(ct)).ToList();
            var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();
            var version = typeof(ServerHost).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
                ?? typeof(ServerHost).Assembly.GetName().Version?.ToString()
                ?? "0.0.0";

            return Results.Ok(new
            {
                status = canConnect && pending.Count == 0 ? "ok" : "degraded",
                version,
                schemaVersion = applied.LastOrDefault(),
                pendingMigrations = pending,
                database = canConnect ? "ok" : "unavailable",
                utcNow = DateTimeOffset.UtcNow,
            });
        });

        app.MapStudentEndpoints();
        app.MapClassEndpoints();
        app.MapLookupEndpoints();
        app.MapEnrollmentEndpoints();

        app.MapFallback("/api/{**path}", (HttpContext context) => ApiProblems.WriteAsync(
            context,
            404,
            "endpoint.not_found",
            "Not found",
            "No API endpoint matches this request."));

        if (hasSpa)
        {
            app.MapFallbackToFile("index.html");
        }
    }

    private static string ResolveUrl(WebApplication app)
    {
        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()
            ?.Addresses;

        var address = addresses?.FirstOrDefault()
            ?? app.Urls.FirstOrDefault()
            ?? throw new InvalidOperationException("The embedded server did not report an address.");

        return address.TrimEnd('/') + "/";
    }
}
