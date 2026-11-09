using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;

namespace TuitionDesk.Application.Classes;

public sealed class UpdateClassHandler(
    IClassRepository repository,
    IClassQueries queries,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<ClassDetail> HandleAsync(
        Guid id,
        SaveClassRequest request,
        CancellationToken cancellationToken = default)
    {
        var classEntity = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("class.not_found", "Class not found.");

        var duplicate = await repository.ExistsSimilarAsync(
            request.Name,
            request.Subject,
            request.ScheduleText,
            excludeClassId: id,
            cancellationToken);

        if (duplicate && !request.AcknowledgeDuplicate)
        {
            throw new ConflictException(
                "class.duplicate_warning",
                "Another active class with the same name, subject, and schedule already exists.");
        }

        classEntity.Update(request.ToDetails(), clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await queries.GetDetailAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("The class was saved but could not be read back.");
    }
}
