using TuitionDesk.Application.Common.Models;
using TuitionDesk.Application.Students;

namespace TuitionDesk.Application.Abstractions;

public interface IStudentQueries
{
    Task<PagedResult<StudentListItem>> SearchAsync(StudentSearch search, CancellationToken cancellationToken = default);

    Task<StudentDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
