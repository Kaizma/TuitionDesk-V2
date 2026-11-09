namespace TuitionDesk.Application.Common.Exceptions;

/// <summary>A requested record does not exist.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
