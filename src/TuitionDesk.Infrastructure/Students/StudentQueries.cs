using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Models;
using TuitionDesk.Application.Students;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Domain.Students;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Infrastructure.Students;

internal sealed class StudentQueries(AppDbContext dbContext) : IStudentQueries
{
    public async Task<PagedResult<StudentListItem>> SearchAsync(
        StudentSearch search,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, search.Page);
        var pageSize = Math.Clamp(search.PageSize, 1, 100);

        var query = dbContext.Students.AsNoTracking();

        query = (search.Status ?? "active").Trim().ToLowerInvariant() switch
        {
            "archived" => query.Where(x => x.Status == StudentStatus.Archived),
            "all" => query,
            _ => query.Where(x => x.Status == StudentStatus.Active),
        };

        var term = search.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term}%";
            query = query.Where(x =>
                EF.Functions.Like(x.FullName, pattern) ||
                EF.Functions.Like(x.StudentCode, pattern) ||
                EF.Functions.Like(x.Phone, pattern));
        }

        if (search.ExcludeClassId is { } excludeClassId)
        {
            query = query.Where(x => !dbContext.Enrollments.Any(e =>
                e.StudentId == x.Id &&
                e.ClassId == excludeClassId &&
                e.Status == EnrollmentStatus.Active));
        }

        var total = await query.CountAsync(cancellationToken);

        var ordered = (search.Sort ?? "name").Trim().ToLowerInvariant() switch
        {
            "code" => query.OrderBy(x => x.StudentCode),
            "recent" => query.OrderByDescending(x => x.UpdatedAtUtc),
            _ => query.OrderBy(x => x.FullName),
        };

        var items = await ordered
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StudentListItem(
                x.Id,
                x.StudentCode,
                x.FullName,
                x.Phone,
                x.GradeLevel,
                x.Status == StudentStatus.Archived ? "archived" : "active",
                dbContext.Enrollments.Count(e =>
                    e.StudentId == x.Id && e.Status == EnrollmentStatus.Active),
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<StudentListItem>(items, page, pageSize, total);
    }

    public async Task<StudentDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.StudentCode,
                x.FullName,
                x.Phone,
                x.Email,
                x.GuardianName,
                x.GuardianPhone,
                x.DateOfBirth,
                x.School,
                x.GradeLevel,
                x.JoinedOn,
                x.Notes,
                x.Status,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
        {
            return null;
        }

        var enrollments = await (
                from enrollment in dbContext.Enrollments.AsNoTracking()
                join classEntity in dbContext.Classes.AsNoTracking()
                    on enrollment.ClassId equals classEntity.Id
                where enrollment.StudentId == id
                orderby enrollment.Status, enrollment.EnrolledOn descending
                select new StudentEnrollment(
                    enrollment.Id,
                    classEntity.Id,
                    classEntity.Name,
                    classEntity.Subject,
                    classEntity.ScheduleText,
                    enrollment.EnrolledOn,
                    enrollment.Status == EnrollmentStatus.Active ? "active" : "withdrawn",
                    enrollment.WithdrawnOn))
            .ToListAsync(cancellationToken);

        return new StudentDetail(
            student.Id,
            student.StudentCode,
            student.FullName,
            student.Phone,
            student.Email,
            student.GuardianName,
            student.GuardianPhone,
            student.DateOfBirth,
            student.School,
            student.GradeLevel,
            student.JoinedOn,
            student.Notes,
            student.Status == StudentStatus.Archived ? "archived" : "active",
            student.CreatedAtUtc,
            student.UpdatedAtUtc,
            enrollments);
    }
}
