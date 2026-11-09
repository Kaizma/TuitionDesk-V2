namespace TuitionDesk.Application.Abstractions;

/// <summary>Provides the current time in a testable way.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }

    /// <summary>The current date in the user's local time zone.</summary>
    DateOnly Today { get; }
}
