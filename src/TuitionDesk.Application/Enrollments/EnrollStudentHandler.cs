using TuitionDesk.Application.Abstractions;
using TuitionDesk.Application.Common.Exceptions;
using TuitionDesk.Domain.Enrollments;

namespace TuitionDesk.Application.Enrollments;

public sealed class EnrollStudentHandler(
    IStudentRepository students,
    IClassRepository classes,
    IEnrollmentRepository enrollments,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<EnrollmentResult> HandleAsync(
        Guid classId,
        EnrollStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await students.GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException("student.not_found", "Student not found.");
        student.EnsureActive();

        var classEntity = await classes.GetByIdAsync(classId, cancellationToken)
            ?? throw new NotFoundException("class.not_found", "Class not found.");
        classEntity.EnsureCanEnroll();

        if (await enrollments.FindActiveAsync(student.Id, classEntity.Id, cancellationToken) is not null)
        {
            throw new ConflictException(
                "enrollment.already_active",
                $"{student.FullName} is already enrolled in {classEntity.Name}.");
        }

        var enrollment = Enrollment.Create(
            student.Id,
            classEntity.Id,
            request.EnrolledOn ?? clock.Today,
            clock.Today,
            clock.UtcNow);

        await enrollments.AddAsync(enrollment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnrollmentResult(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.ClassId,
            enrollment.EnrolledOn,
            "active");
    }
}
