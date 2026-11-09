namespace TuitionDesk.Server.Hosting;

public sealed record ServerOptions
{
    public required string DataDirectory { get; init; }

    public string[] Urls { get; init; } = ["http://127.0.0.1:5179"];

    public bool RequireToken { get; init; }

    public string? Token { get; init; }

    public string? ContentRootPath { get; init; }

    public bool ServeSpa { get; init; } = true;

    public string EnvironmentName { get; init; } = "Production";
}
