using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Students;

public sealed class GetStudentHandler(IStudentQueries queries)
{
    public async Task<StudentDetail> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await queries.GetDetailAsync(id, cancellationToken)
            ?? throw new NotFoundException("student.not_found", "Student not found.");
    }
}
