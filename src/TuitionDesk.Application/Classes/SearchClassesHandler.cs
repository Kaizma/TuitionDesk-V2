using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Models;

namespace TuitionDesk.Application.Classes;

public sealed class SearchClassesHandler(IClassQueries queries)
{
    public Task<PagedResult<ClassListItem>> HandleAsync(
        ClassSearch search,
        CancellationToken cancellationToken = default)
    {
        return queries.SearchAsync(search, cancellationToken);
    }
}
