namespace TuitionDesk.Domain.Students;

/// <summary>The editable profile of a student. Codes, status, and timestamps are managed by the entity.</summary>
public sealed record StudentDetails(
    string FullName,
    string Phone,
    string? Email,
    string? GuardianName,
    string? GuardianPhone,
    DateOnly? DateOfBirth,
    string? School,
    string? GradeLevel,
    DateOnly JoinedOn,
    string? Notes);
