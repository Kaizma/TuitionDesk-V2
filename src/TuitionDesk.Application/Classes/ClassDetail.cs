namespace TuitionDesk.Application.Classes;

public sealed record ClassDetail(
    Guid Id,
    string Code,
    string Name,
    string Subject,
    string? GradeLevel,
    string? ScheduleText,
    DateOnly StartDate,
    DateOnly? EndDate,
    long MonthlyFeeMinor,
    string Currency,
    int? Capacity,
    string? Notes,
    string Status,
    int ActiveEnrollmentCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
