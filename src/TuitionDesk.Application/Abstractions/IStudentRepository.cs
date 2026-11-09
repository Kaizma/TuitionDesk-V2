using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Domain.Students;

namespace TuitionDesk.Application.Abstractions;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task AddAsync(Student student, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Enrollment>> GetActiveEnrollmentsAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsSimilarAsync(
        string fullName,
        string phone,
        Guid? excludeStudentId = null,
        CancellationToken cancellationToken = default);
}
