namespace TuitionDesk.Application.Classes;

public sealed record SaveClassRequest(
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
    bool AcknowledgeDuplicate = false);
