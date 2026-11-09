using TuitionDesk.Domain.Students;

namespace TuitionDesk.Application.Students;

internal static class StudentMappings
{
    public static StudentDetails ToDetails(this SaveStudentRequest request)
    {
        return new StudentDetails(
            request.FullName,
            request.Phone,
            request.Email,
            request.GuardianName,
            request.GuardianPhone,
            request.DateOfBirth,
            request.School,
            request.GradeLevel,
            request.JoinedOn,
            request.Notes);
    }

    public static string ToStatusLabel(this StudentStatus status)
        => status == StudentStatus.Archived ? "archived" : "active";
}
