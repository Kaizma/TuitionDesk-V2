namespace TuitionDesk.Application.Classes;

public sealed record ClassSearch(
    string? Search,
    string Status = "active",
    string? Subject = null,
    string Sort = "name",
    int Page = 1,
    int PageSize = 25,
    Guid? ExcludeStudentId = null);
