namespace TuitionDesk.Application.Classes;

public sealed record ClassListItem(
    Guid Id,
    string Code,
    string Name,
    string Subject,
    string? GradeLevel,
    string? ScheduleText,
    int ActiveEnrollmentCount,
    int? Capacity,
    long MonthlyFeeMinor,
    string Currency,
    string Status);
