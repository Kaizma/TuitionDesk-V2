using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Classes;

public sealed class GetClassHandler(IClassQueries queries)
{
    public async Task<ClassDetail> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await queries.GetDetailAsync(id, cancellationToken)
            ?? throw new NotFoundException("class.not_found", "Class not found.");
    }
}
