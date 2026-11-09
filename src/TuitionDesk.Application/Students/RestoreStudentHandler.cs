using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Students;

public sealed class RestoreStudentHandler(
    IStudentRepository repository,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("student.not_found", "Student not found.");

        student.Restore(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
