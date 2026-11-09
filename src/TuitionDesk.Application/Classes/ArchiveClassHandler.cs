using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Classes;

public sealed class ArchiveClassHandler(
    IClassRepository repository,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var classEntity = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("class.not_found", "Class not found.");

        var activeEnrollments = await repository.GetActiveEnrollmentsAsync(id, cancellationToken);

        classEntity.Archive(clock.UtcNow);

        foreach (var enrollment in activeEnrollments)
        {
            enrollment.Withdraw(clock.Today, clock.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
