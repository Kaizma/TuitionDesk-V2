using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Infrastructure.Enrollments;

internal sealed class EnrollmentRepository(AppDbContext dbContext) : IEnrollmentRepository
{
    public Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Enrollments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Enrollment?> FindActiveAsync(
        Guid studentId,
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Enrollments.FirstOrDefaultAsync(
            x => x.StudentId == studentId
                 && x.ClassId == classId
                 && x.Status == EnrollmentStatus.Active,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetActiveForClassAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Enrollments
            .Where(x => x.ClassId == classId && x.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        return dbContext.Enrollments.AddAsync(enrollment, cancellationToken).AsTask();
    }

    public Task AddRangeAsync(
        IEnumerable<Enrollment> enrollments,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Enrollments.AddRangeAsync(enrollments, cancellationToken);
    }
}
