using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Classes;

public sealed class RestoreClassHandler(
    IClassRepository repository,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var classEntity = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("class.not_found", "Class not found.");

        classEntity.Restore(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
