namespace TuitionDesk.Application.Students;

public sealed record StudentListItem(
    Guid Id,
    string Code,
    string FullName,
    string Phone,
    string? GradeLevel,
    string Status,
    int ActiveClassCount,
    DateTimeOffset UpdatedAt);
