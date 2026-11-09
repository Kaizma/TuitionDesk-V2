using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TuitionDesk.Server.Http;

internal static class ApiProblems
{
    public static Task WriteAsync(
        HttpContext context,
        int status,
        string code,
        string title,
        string? detail = null,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://tuitiondesk.local/errors/{code}",
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return Results.Problem(problem).ExecuteAsync(context);
    }
}
