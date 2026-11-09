using FluentValidation;
using TuitionDesk.Application.Abstractions;

namespace TuitionDesk.Application.Students;

public sealed class SaveStudentValidator : AbstractValidator<SaveStudentRequest>
{
    public SaveStudentValidator(IClock clock)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .Length(2, 120).WithMessage("Full name must be between 2 and 120 characters.")
            .Must(name => name.Any(char.IsLetter)).WithMessage("Full name must contain at least one letter.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Length(7, 20).WithMessage("Phone must be between 7 and 20 characters.")
            .Matches(@"^[0-9+\-() ]+$")
            .WithMessage("Phone may only contain digits, spaces, and + - ( ) characters.");

        RuleFor(x => x.Email)
            .MaximumLength(200).WithMessage("Email must be 200 characters or fewer.")
            .EmailAddress().WithMessage("Enter a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.GuardianName)
            .MaximumLength(120).WithMessage("Guardian name must be 120 characters or fewer.");

        RuleFor(x => x.GuardianPhone)
            .Length(7, 20).WithMessage("Guardian phone must be between 7 and 20 characters.")
            .Matches(@"^[0-9+\-() ]+$")
            .WithMessage("Guardian phone may only contain digits, spaces, and + - ( ) characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.GuardianPhone));

        RuleFor(x => x.DateOfBirth)
            .Must(date => date is null || date <= clock.Today)
            .WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.JoinedOn)
            .Must(date => date <= clock.Today)
            .WithMessage("Joined date cannot be in the future.");

        RuleFor(x => x.School)
            .MaximumLength(120).WithMessage("School must be 120 characters or fewer.");

        RuleFor(x => x.GradeLevel)
            .MaximumLength(40).WithMessage("Grade level must be 40 characters or fewer.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must be 1,000 characters or fewer.");
    }
}
