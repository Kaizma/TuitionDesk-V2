using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Models;

namespace TuitionDesk.Application.Students;

public sealed class SearchStudentsHandler(IStudentQueries queries)
{
    public Task<PagedResult<StudentListItem>> HandleAsync(
        StudentSearch search,
        CancellationToken cancellationToken = default)
    {
        return queries.SearchAsync(search, cancellationToken);
    }
}
