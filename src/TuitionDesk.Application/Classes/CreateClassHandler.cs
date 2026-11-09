using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;
using TuitionDesk.Domain.Classes;

namespace TuitionDesk.Application.Classes;

public sealed class CreateClassHandler(
    IClassRepository repository,
    IClassQueries queries,
    ICounterService counterService,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<ClassDetail> HandleAsync(SaveClassRequest request, CancellationToken cancellationToken = default)
    {
        var duplicate = await repository.ExistsSimilarAsync(
            request.Name,
            request.Subject,
            request.ScheduleText,
            excludeClassId: null,
            cancellationToken);

        if (duplicate && !request.AcknowledgeDuplicate)
        {
            throw new ConflictException(
                "class.duplicate_warning",
                "An active class with the same name, subject, and schedule already exists.");
        }

        var code = await counterService.NextClassCodeAsync(cancellationToken);
        var classEntity = Class.Create(code, request.ToDetails(), clock.UtcNow);

        await repository.AddAsync(classEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await queries.GetDetailAsync(classEntity.Id, cancellationToken)
            ?? throw new InvalidOperationException("The class was saved but could not be read back.");
    }
}
