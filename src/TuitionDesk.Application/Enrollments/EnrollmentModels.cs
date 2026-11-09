namespace TuitionDesk.Application.Enrollments;

public sealed record EnrollmentResult(
    Guid Id,
    Guid StudentId,
    Guid ClassId,
    DateOnly EnrolledOn,
    string Status);

public sealed record EnrollStudentRequest(Guid StudentId, DateOnly? EnrolledOn);

public sealed record EnrollStudentsRequest(IReadOnlyList<Guid> StudentIds, DateOnly? EnrolledOn);

public sealed record RosterSearch(
    Guid ClassId,
    string? Search,
    string Status = "active",
    int Page = 1,
    int PageSize = 25);

public sealed record RosterItem(
    Guid EnrollmentId,
    Guid StudentId,
    string StudentCode,
    string FullName,
    string Phone,
    string? GradeLevel,
    DateOnly EnrolledOn,
    string Status,
    DateOnly? WithdrawnOn,
    string StudentStatus);
