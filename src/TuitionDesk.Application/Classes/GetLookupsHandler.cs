using TuitionDesk.Application.Abstractions;

namespace TuitionDesk.Application.Classes;

public sealed class GetLookupsHandler(ILookupQueries queries)
{
    public Task<Lookups> HandleAsync(CancellationToken cancellationToken = default)
        => queries.GetAsync(cancellationToken);
}
