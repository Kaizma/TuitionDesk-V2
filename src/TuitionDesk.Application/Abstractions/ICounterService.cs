namespace TuitionDesk.Application.Abstractions;

public interface ICounterService
{
    /// <summary>Allocates the next student code without saving; the caller commits it with the student.</summary>
    Task<string> NextStudentCodeAsync(CancellationToken cancellationToken = default);

    /// <summary>Allocates the next class code without saving; the caller commits it with the class.</summary>
    Task<string> NextClassCodeAsync(CancellationToken cancellationToken = default);
}
