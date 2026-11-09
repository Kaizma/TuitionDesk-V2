using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Classes;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Infrastructure.Classes;

internal sealed class LookupQueries(AppDbContext dbContext) : ILookupQueries
{
    public async Task<Lookups> GetAsync(CancellationToken cancellationToken = default)
    {
        var subjects = await dbContext.Classes
            .AsNoTracking()
            .Where(x => x.Subject != string.Empty)
            .Select(x => x.Subject)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var classGrades = await dbContext.Classes
            .AsNoTracking()
            .Where(x => x.GradeLevel != null && x.GradeLevel != string.Empty)
            .Select(x => x.GradeLevel!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var studentGrades = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.GradeLevel != null && x.GradeLevel != string.Empty)
            .Select(x => x.GradeLevel!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var grades = classGrades
            .Concat(studentGrades)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        return new Lookups(subjects, grades);
    }
}
