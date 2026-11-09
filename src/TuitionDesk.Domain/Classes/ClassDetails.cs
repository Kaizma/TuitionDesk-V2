namespace TuitionDesk.Domain.Classes;

/// <summary>The editable definition of a class. Codes, status, and timestamps are managed by the entity.</summary>
public sealed record ClassDetails(
    string Name,
    string Subject,
    string? GradeLevel,
    string? ScheduleText,
    DateOnly StartDate,
    DateOnly? EndDate,
    long MonthlyFeeMinor,
    string Currency,
    int? Capacity,
    string? Notes);
