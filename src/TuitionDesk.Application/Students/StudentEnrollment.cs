namespace TuitionDesk.Application.Students;

public sealed record StudentEnrollment(
    Guid Id,
    Guid ClassId,
    string ClassName,
    string Subject,
    string? ScheduleText,
    DateOnly EnrolledOn,
    string Status,
    DateOnly? WithdrawnOn);
