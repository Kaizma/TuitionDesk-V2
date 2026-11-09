using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;
using TuitionDesk.Domain.Enrollments;

namespace TuitionDesk.Application.Students;

public sealed class ArchiveStudentHandler(
    IStudentRepository repository,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("student.not_found", "Student not found.");

        var activeEnrollments = await repository.GetActiveEnrollmentsAsync(id, cancellationToken);

        student.Archive(clock.UtcNow);

        foreach (var enrollment in activeEnrollments)
        {
            enrollment.Withdraw(clock.Today, clock.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
