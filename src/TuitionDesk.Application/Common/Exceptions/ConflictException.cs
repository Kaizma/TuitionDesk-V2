namespace TuitionDesk.Application.Common.Exceptions;

/// <summary>The operation conflicts with the current state of the data.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
