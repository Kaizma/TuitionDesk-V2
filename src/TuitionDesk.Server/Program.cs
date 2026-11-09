using TuitionDesk.Infrastructure.Persistence;
using TuitionDesk.Server.Hosting;

var dataDirectory = DataPaths.ResolveRoot();

var options = new ServerOptions
{
    DataDirectory = dataDirectory,
    Urls = [Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://127.0.0.1:5179"],
    RequireToken = false,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
    ServeSpa = false,
};

await using var server = await ServerHost.StartAsync(options);
Console.WriteLine($"TuitionDesk API listening at {server.Url}");
await server.WaitForShutdownAsync();

public partial class Program
{
}
