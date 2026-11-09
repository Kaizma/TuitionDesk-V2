using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Classes;
using TuitionDesk.Application.Common.Models;
using TuitionDesk.Application.Enrollments;
using TuitionDesk.Domain.Classes;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Domain.Students;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Infrastructure.Classes;

internal sealed class ClassQueries(AppDbContext dbContext) : IClassQueries
{
    public async Task<PagedResult<ClassListItem>> SearchAsync(
        ClassSearch search,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, search.Page);
        var pageSize = Math.Clamp(search.PageSize, 1, 100);

        var query = dbContext.Classes.AsNoTracking();

        query = (search.Status ?? "active").Trim().ToLowerInvariant() switch
        {
            "archived" => query.Where(x => x.Status == ClassStatus.Archived),
            "all" => query,
            _ => query.Where(x => x.Status == ClassStatus.Active),
        };

        var subject = search.Subject?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(subject))
        {
            query = query.Where(x => x.Subject.ToLower() == subject);
        }

        var term = search.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Name, pattern) ||
                EF.Functions.Like(x.Subject, pattern) ||
                EF.Functions.Like(x.ClassCode, pattern));
        }

        if (search.ExcludeStudentId is { } excludeStudentId)
        {
            query = query.Where(x => !dbContext.Enrollments.Any(e =>
                e.ClassId == x.Id &&
                e.StudentId == excludeStudentId &&
                e.Status == EnrollmentStatus.Active));
        }

        var total = await query.CountAsync(cancellationToken);

        var ordered = (search.Sort ?? "name").Trim().ToLowerInvariant() switch
        {
            "code" => query.OrderBy(x => x.ClassCode),
            "recent" => query.OrderByDescending(x => x.UpdatedAtUtc),
            _ => query.OrderBy(x => x.Name),
        };

        var items = await ordered
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ClassListItem(
                x.Id,
                x.ClassCode,
                x.Name,
                x.Subject,
                x.GradeLevel,
                x.ScheduleText,
                dbContext.Enrollments.Count(e =>
                    e.ClassId == x.Id && e.Status == EnrollmentStatus.Active),
                x.Capacity,
                x.MonthlyFeeMinor,
                x.Currency,
                x.Status == ClassStatus.Archived ? "archived" : "active"))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClassListItem>(items, page, pageSize, total);
    }

    public async Task<ClassDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Classes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ClassDetail(
                x.Id,
                x.ClassCode,
                x.Name,
                x.Subject,
                x.GradeLevel,
                x.ScheduleText,
                x.StartDate,
                x.EndDate,
                x.MonthlyFeeMinor,
                x.Currency,
                x.Capacity,
                x.Notes,
                x.Status == ClassStatus.Archived ? "archived" : "active",
                dbContext.Enrollments.Count(e =>
                    e.ClassId == x.Id && e.Status == EnrollmentStatus.Active),
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<RosterItem>> GetRosterAsync(
        RosterSearch search,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, search.Page);
        var pageSize = Math.Clamp(search.PageSize, 1, 100);

        var query =
            from enrollment in dbContext.Enrollments.AsNoTracking()
            join student in dbContext.Students.AsNoTracking()
                on enrollment.StudentId equals student.Id
            where enrollment.ClassId == search.ClassId
            select new { Enrollment = enrollment, Student = student };

        query = (search.Status ?? "active").Trim().ToLowerInvariant() switch
        {
            "withdrawn" => query.Where(x => x.Enrollment.Status == EnrollmentStatus.Withdrawn),
            "all" => query,
            _ => query.Where(x => x.Enrollment.Status == EnrollmentStatus.Active),
        };

        var term = search.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Student.FullName, pattern) ||
                EF.Functions.Like(x.Student.StudentCode, pattern) ||
                EF.Functions.Like(x.Student.Phone, pattern));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Enrollment.Status)
            .ThenBy(x => x.Student.FullName)
            .ThenBy(x => x.Enrollment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RosterItem(
                x.Enrollment.Id,
                x.Student.Id,
                x.Student.StudentCode,
                x.Student.FullName,
                x.Student.Phone,
                x.Student.GradeLevel,
                x.Enrollment.EnrolledOn,
                x.Enrollment.Status == EnrollmentStatus.Active ? "active" : "withdrawn",
                x.Enrollment.WithdrawnOn,
                x.Student.Status == StudentStatus.Archived ? "archived" : "active"))
            .ToListAsync(cancellationToken);

        return new PagedResult<RosterItem>(items, page, pageSize, total);
    }
}
