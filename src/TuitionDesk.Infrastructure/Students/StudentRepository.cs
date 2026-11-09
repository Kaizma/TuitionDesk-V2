using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Domain.Students;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Infrastructure.Students;

internal sealed class StudentRepository(AppDbContext dbContext) : IStudentRepository
{
    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Students.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Students
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        return dbContext.Students.AddAsync(student, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<Enrollment>> GetActiveEnrollmentsAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Enrollments
            .Where(x => x.StudentId == studentId && x.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsSimilarAsync(
        string fullName,
        string phone,
        Guid? excludeStudentId = null,
        CancellationToken cancellationToken = default)
    {
        var name = fullName.Trim().ToLowerInvariant();
        var normalizedPhone = phone.Trim();

        return dbContext.Students.AnyAsync(
            x => x.Status == StudentStatus.Active
                 && x.FullName.ToLower() == name
                 && x.Phone == normalizedPhone
                 && (excludeStudentId == null || x.Id != excludeStudentId),
            cancellationToken);
    }
}
