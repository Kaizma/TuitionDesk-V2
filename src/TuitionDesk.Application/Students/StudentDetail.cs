namespace TuitionDesk.Application.Students;

public sealed record StudentDetail(
    Guid Id,
    string Code,
    string FullName,
    string Phone,
    string? Email,
    string? GuardianName,
    string? GuardianPhone,
    DateOnly? DateOfBirth,
    string? School,
    string? GradeLevel,
    DateOnly JoinedOn,
    string? Notes,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<StudentEnrollment> Enrollments);
