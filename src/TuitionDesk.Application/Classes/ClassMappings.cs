using TuitionDesk.Domain.Classes;

namespace TuitionDesk.Application.Classes;

internal static class ClassMappings
{
    public static ClassDetails ToDetails(this SaveClassRequest request)
    {
        return new ClassDetails(
            request.Name,
            request.Subject,
            request.GradeLevel,
            request.ScheduleText,
            request.StartDate,
            request.EndDate,
            request.MonthlyFeeMinor,
            request.Currency,
            request.Capacity,
            request.Notes);
    }

    public static string ToStatusLabel(this ClassStatus status)
        => status == ClassStatus.Archived ? "archived" : "active";
}
