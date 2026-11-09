using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Domain.Classes;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Infrastructure.Classes;

internal sealed class ClassRepository(AppDbContext dbContext) : IClassRepository
{
    public Task<Class?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Classes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(Class classEntity, CancellationToken cancellationToken = default)
    {
        return dbContext.Classes.AddAsync(classEntity, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<Enrollment>> GetActiveEnrollmentsAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Enrollments
            .Where(x => x.ClassId == classId && x.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsSimilarAsync(
        string name,
        string subject,
        string? scheduleText,
        Guid? excludeClassId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        var normalizedSubject = subject.Trim().ToLowerInvariant();
        var normalizedSchedule = string.IsNullOrWhiteSpace(scheduleText)
            ? null
            : scheduleText.Trim().ToLowerInvariant();

        return dbContext.Classes.AnyAsync(
            x => x.Status == ClassStatus.Active
                 && x.Name.ToLower() == normalizedName
                 && x.Subject.ToLower() == normalizedSubject
                 && (normalizedSchedule == null
                     ? x.ScheduleText == null || x.ScheduleText == string.Empty
                     : x.ScheduleText != null && x.ScheduleText.ToLower() == normalizedSchedule)
                 && (excludeClassId == null || x.Id != excludeClassId),
            cancellationToken);
    }
}
