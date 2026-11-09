using System.Text.Json;
using FluentValidation;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Server.Http;

/// <summary>Validates endpoint request bodies and returns the standard field-level error shape.</summary>
public sealed class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (result.IsValid)
        {
            return await next(context);
        }

        var errors = result.Errors
            .GroupBy(
                failure => JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray(),
                StringComparer.Ordinal);

        throw new RequestValidationException(errors);
    }
}
