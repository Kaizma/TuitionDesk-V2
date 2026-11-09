using System.Net;
using System.Security.Cryptography;
using System.Text;
using TuitionDesk.Server.Hosting;
using TuitionDesk.Server.Http;

namespace TuitionDesk.Server.Middleware;

/// <summary>Rejects API calls that do not come from the desktop shell with the per-launch token.</summary>
public sealed class LoopbackTokenMiddleware(RequestDelegate next, ServerOptions options)
{
    private const string HeaderName = "X-TuitionDesk-Token";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        if (!IsLoopbackHost(context.Request.Host.Host))
        {
            await ApiProblems.WriteAsync(
                context,
                403,
                "security.loopback_only",
                "Forbidden",
                "This API only accepts requests from the local machine.");
            return;
        }

        var origin = context.Request.Headers.Origin.ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            var expectedOrigin = $"{context.Request.Scheme}://{context.Request.Host}";
            if (!string.Equals(origin, expectedOrigin, StringComparison.OrdinalIgnoreCase))
            {
                await ApiProblems.WriteAsync(
                    context,
                    403,
                    "security.origin_rejected",
                    "Forbidden",
                    "Cross-origin requests are not accepted.");
                return;
            }
        }

        var provided = context.Request.Headers[HeaderName].ToString();
        var expected = options.Token ?? string.Empty;

        if (string.IsNullOrEmpty(expected) || !TokensMatch(provided, expected))
        {
            await ApiProblems.WriteAsync(
                context,
                401,
                "security.invalid_token",
                "Unauthorized",
                "The application session token is missing or invalid.");
            return;
        }

        await next(context);
    }

    private static bool IsLoopbackHost(string host)
    {
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IPAddress.TryParse(host.Trim('[', ']'), out var address) && IPAddress.IsLoopback(address);
    }

    private static bool TokensMatch(string provided, string expected)
    {
        var left = Encoding.UTF8.GetBytes(provided);
        var right = Encoding.UTF8.GetBytes(expected);
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }
}
