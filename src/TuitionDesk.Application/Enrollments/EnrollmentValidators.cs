using FluentValidation;

namespace TuitionDesk.Application.Enrollments;

public sealed class EnrollStudentValidator : AbstractValidator<EnrollStudentRequest>
{
    public EnrollStudentValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("A student is required.");
    }
}

public sealed class EnrollStudentsValidator : AbstractValidator<EnrollStudentsRequest>
{
    public EnrollStudentsValidator()
    {
        RuleFor(x => x.StudentIds)
            .NotNull().WithMessage("Select at least one student.")
            .Must(ids => ids is { Count: > 0 }).WithMessage("Select at least one student.")
            .Must(ids => ids.Count <= 100).WithMessage("Select no more than 100 students at once.")
            .Must(ids => ids.All(id => id != Guid.Empty)).WithMessage("Every selected student is invalid.");
    }
}
