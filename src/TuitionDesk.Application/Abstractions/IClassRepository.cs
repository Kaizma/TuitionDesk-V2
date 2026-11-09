using TuitionDesk.Application.Classes;
using TuitionDesk.Application.Common.Models;
using TuitionDesk.Application.Enrollments;
using TuitionDesk.Domain.Classes;
using TuitionDesk.Domain.Enrollments;

namespace TuitionDesk.Application.Abstractions;

public interface IClassRepository
{
    Task<Class?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Class classEntity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Enrollment>> GetActiveEnrollmentsAsync(
        Guid classId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsSimilarAsync(
        string name,
        string subject,
        string? scheduleText,
        Guid? excludeClassId = null,
        CancellationToken cancellationToken = default);
}

public interface IClassQueries
{
    Task<PagedResult<ClassListItem>> SearchAsync(
        ClassSearch search,
        CancellationToken cancellationToken = default);

    Task<ClassDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<RosterItem>> GetRosterAsync(
        RosterSearch search,
        CancellationToken cancellationToken = default);
}

public interface ILookupQueries
{
    Task<Lookups> GetAsync(CancellationToken cancellationToken = default);
}
