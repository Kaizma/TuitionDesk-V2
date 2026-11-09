namespace TuitionDesk.Domain.Common;

/// <summary>Base class for rule violations raised by the domain model.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>Stable, machine-readable error code (for example <c>student.archived</c>).</summary>
    public string Code { get; }
}

/// <summary>The supplied values violate an invariant (bad input).</summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string code, string message)
        : base(code, message)
    {
    }
}

/// <summary>The operation is invalid for the entity's current state.</summary>
public sealed class DomainStateException : DomainException
{
    public DomainStateException(string code, string message)
        : base(code, message)
    {
    }
}
