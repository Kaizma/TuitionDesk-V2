namespace TuitionDesk.Server.Hosting;

/// <summary>A running embedded API. Disposing stops the server.</summary>
public sealed class ServerInstance(WebApplication app, string url, string token) : IAsyncDisposable
{
    public string Url { get; } = url;

    public string Token { get; } = token;

    public WebApplication App { get; } = app;

    public Task WaitForShutdownAsync(CancellationToken cancellationToken = default)
        => App.WaitForShutdownAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await App.StopAsync(timeout.Token);
        }
        catch (Exception)
        {
            // Shutdown is best effort; the process is going away either way.
        }

        await App.DisposeAsync();
    }
}
