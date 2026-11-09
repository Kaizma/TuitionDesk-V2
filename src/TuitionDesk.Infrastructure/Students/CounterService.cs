using Microsoft.EntityFrameworkCore;
using TuitionDesk.Application.Abstractions;
using TuitionDesk.Domain.Classes;
using TuitionDesk.Domain.Students;
using TuitionDesk.Infrastructure.Persistence;

namespace TuitionDesk.Infrastructure.Students;

internal sealed class CounterService(AppDbContext dbContext) : ICounterService
{
    private const string StudentCounterKey = "student-code";
    private const string ClassCounterKey = "class-code";

    public Task<string> NextStudentCodeAsync(CancellationToken cancellationToken = default)
        => NextAsync(StudentCounterKey, StudentCodes.Format, cancellationToken);

    public Task<string> NextClassCodeAsync(CancellationToken cancellationToken = default)
        => NextAsync(ClassCounterKey, ClassCodes.Format, cancellationToken);

    private async Task<string> NextAsync(
        string key,
        Func<int, string> format,
        CancellationToken cancellationToken)
    {
        var counter = await dbContext.Counters
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        if (counter is null)
        {
            counter = new Counter { Key = key, NextValue = 1 };
            await dbContext.Counters.AddAsync(counter, cancellationToken);
        }

        var next = counter.NextValue;
        if (next > int.MaxValue)
        {
            throw new InvalidOperationException($"The {key} counter is exhausted.");
        }

        counter.NextValue = next + 1;
        return format((int)next);
    }
}
