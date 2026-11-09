using System.Net;
using Xunit;

namespace TuitionDesk.Server.IntegrationTests;

public sealed class SecurityTests
{
    [Fact]
    public async Task Token_protected_api_rejects_missing_and_accepts_matching_token()
    {
        await using var app = await TestApp.StartAsync(requireToken: true);

        var withoutToken = await app.Client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.Unauthorized, withoutToken.StatusCode);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        request.Headers.Add("X-TuitionDesk-Token", app.Server.Token);

        var withToken = await app.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, withToken.StatusCode);
    }
}
