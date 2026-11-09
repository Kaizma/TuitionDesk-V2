namespace TuitionDesk.Application.Students;

/// <summary>Filter and paging input for the student list. Status and sort are validated by the query service.</summary>
public sealed record StudentSearch(
    string? Search,
    string Status = "active",
    string Sort = "name",
    int Page = 1,
    int PageSize = 25,
    Guid? ExcludeClassId = null);
