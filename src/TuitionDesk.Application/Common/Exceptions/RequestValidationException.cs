namespace TuitionDesk.Application.Common.Exceptions;

/// <summary>Input failed validation. Carries field-level messages.</summary>
public sealed class RequestValidationException : Exception
{
    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public string Code => "validation_failed";

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
