namespace TuitionDesk.Application.Students;

public sealed record SaveStudentRequest(
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
    bool AcknowledgeDuplicate = false);
