using TuitionDesk.Domain.Enrollments;

namespace TuitionDesk.Application.Abstractions;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Enrollment?> FindActiveAsync(
        Guid studentId,
        Guid classId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Enrollment>> GetActiveForClassAsync(
        Guid classId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<Enrollment> enrollments,
        CancellationToken cancellationToken = default);
}
