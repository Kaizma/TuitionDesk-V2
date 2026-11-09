using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Common.Exceptions;
using TuitionDesk.Domain.Common;
using TuitionDesk.Server.Http;

namespace TuitionDesk.Server.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client went away; nothing to report.
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "Unhandled exception after the response had started.");
            throw exception;
        }

        switch (exception)
        {
            case RequestValidationException validation:
                await ApiProblems.WriteAsync(
                    context,
                    400,
                    validation.Code,
                    "One or more validation errors occurred.",
                    errors: validation.Errors);
                break;

            case DomainValidationException domainValidation:
                await ApiProblems.WriteAsync(context, 400, domainValidation.Code, "Invalid request", domainValidation.Message);
                break;

            case DomainStateException domainState:
                await ApiProblems.WriteAsync(context, 409, domainState.Code, "Conflict", domainState.Message);
                break;

            case ConflictException conflict:
                await ApiProblems.WriteAsync(context, 409, conflict.Code, "Conflict", conflict.Message);
                break;

            case NotFoundException notFound:
                await ApiProblems.WriteAsync(context, 404, notFound.Code, "Not found", notFound.Message);
                break;

            case BadHttpRequestException badRequest:
                await ApiProblems.WriteAsync(context, 400, "request.invalid", "Invalid request", badRequest.Message);
                break;

            case DbUpdateException dbUpdate:
                logger.LogWarning(dbUpdate, "Database constraint violation.");
                await ApiProblems.WriteAsync(
                    context,
                    409,
                    "database.constraint",
                    "Conflict",
                    "The change conflicts with existing data. Refresh and try again.");
                break;

            default:
                logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
                await ApiProblems.WriteAsync(
                    context,
                    500,
                    "server.unexpected",
                    "Something went wrong",
                    "The action could not be completed. Check the logs for details.");
                break;
        }
    }
}
