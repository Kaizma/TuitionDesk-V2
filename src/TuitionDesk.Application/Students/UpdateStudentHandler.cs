using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Students;

public sealed class UpdateStudentHandler(
    IStudentRepository repository,
    IStudentQueries queries,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<StudentDetail> HandleAsync(
        Guid id,
        SaveStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("student.not_found", "Student not found.");

        var duplicate = await repository.ExistsSimilarAsync(
            request.FullName,
            request.Phone,
            excludeStudentId: id,
            cancellationToken);

        if (duplicate && !request.AcknowledgeDuplicate)
        {
            throw new ConflictException(
                "student.duplicate_warning",
                "Another student with this name and phone number already exists.");
        }

        student.Update(request.ToDetails(), clock.Today, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await queries.GetDetailAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("The student was saved but could not be read back.");
    }
}
