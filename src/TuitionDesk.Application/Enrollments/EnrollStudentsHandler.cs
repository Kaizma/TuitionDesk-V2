using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;
using TuitionDesk.Domain.Enrollments;

namespace TuitionDesk.Application.Enrollments;

public sealed class EnrollStudentsHandler(
    IStudentRepository students,
    IClassRepository classes,
    IEnrollmentRepository enrollments,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<int> HandleAsync(
        Guid classId,
        EnrollStudentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var studentIds = request.StudentIds.Distinct().ToList();

        var classEntity = await classes.GetByIdAsync(classId, cancellationToken)
            ?? throw new NotFoundException("class.not_found", "Class not found.");
        classEntity.EnsureCanEnroll();

        var studentEntities = await students.GetByIdsAsync(studentIds, cancellationToken);
        if (studentEntities.Count != studentIds.Count)
        {
            throw new NotFoundException("student.not_found", "One or more selected students were not found.");
        }

        var archived = studentEntities.Where(x => x.Status == Domain.Students.StudentStatus.Archived).ToList();
        if (archived.Count > 0)
        {
            throw new ConflictException(
                "student.archived",
                $"Archived students cannot be enrolled: {string.Join(", ", archived.Select(x => x.FullName))}.");
        }

        var existing = await enrollments.GetActiveForClassAsync(classId, cancellationToken);
        var existingStudentIds = existing.Select(x => x.StudentId).ToHashSet();
        var alreadyEnrolled = studentEntities.Where(x => existingStudentIds.Contains(x.Id)).ToList();

        if (alreadyEnrolled.Count > 0)
        {
            throw new ConflictException(
                "enrollment.already_active",
                $"Already enrolled: {string.Join(", ", alreadyEnrolled.Select(x => x.FullName))}.");
        }

        var created = new List<Enrollment>(studentEntities.Count);
        foreach (var student in studentEntities)
        {
            var enrollment = Enrollment.Create(
                student.Id,
                classEntity.Id,
                request.EnrolledOn ?? clock.Today,
                clock.Today,
                clock.UtcNow);

            created.Add(enrollment);
        }

        await enrollments.AddRangeAsync(created, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return created.Count;
    }
}
