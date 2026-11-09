using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;
using TuitionDesk.Domain.Students;

namespace TuitionDesk.Application.Students;

public sealed class CreateStudentHandler(
    IStudentRepository repository,
    IStudentQueries queries,
    ICounterService counterService,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<StudentDetail> HandleAsync(SaveStudentRequest request, CancellationToken cancellationToken = default)
    {
        var duplicate = await repository.ExistsSimilarAsync(
            request.FullName,
            request.Phone,
            excludeStudentId: null,
            cancellationToken);

        if (duplicate && !request.AcknowledgeDuplicate)
        {
            throw new ConflictException(
                "student.duplicate_warning",
                "A student with this name and phone number already exists.");
        }

        var code = await counterService.NextStudentCodeAsync(cancellationToken);
        var student = Student.Create(code, request.ToDetails(), clock.Today, clock.UtcNow);

        await repository.AddAsync(student, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await queries.GetDetailAsync(student.Id, cancellationToken)
            ?? throw new InvalidOperationException("The student was saved but could not be read back.");
    }
}
