using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Enrollments;

public sealed class WithdrawEnrollmentHandler(
    IEnrollmentRepository enrollments,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollments.GetByIdAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("enrollment.not_found", "Enrolment not found.");

        enrollment.Withdraw(clock.Today, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
