using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Models;

namespace TuitionDesk.Application.Enrollments;

public sealed class GetClassRosterHandler(IClassQueries queries)
{
    public Task<PagedResult<RosterItem>> HandleAsync(
        RosterSearch search,
        CancellationToken cancellationToken = default)
    {
        return queries.GetRosterAsync(search, cancellationToken);
    }
}
