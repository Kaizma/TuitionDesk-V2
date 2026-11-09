using FluentValidation;

namespace TuitionDesk.Application.Classes;

public sealed class SaveClassValidator : AbstractValidator<SaveClassRequest>
{
    public SaveClassValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Class name is required.")
            .Length(2, 120).WithMessage("Class name must be between 2 and 120 characters.")
            .Must(name => name.Any(char.IsLetter)).WithMessage("Class name must contain at least one letter.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .Length(2, 60).WithMessage("Subject must be between 2 and 60 characters.");

        RuleFor(x => x.GradeLevel)
            .MaximumLength(40).WithMessage("Grade level must be 40 characters or fewer.");

        RuleFor(x => x.ScheduleText)
            .MaximumLength(200).WithMessage("Schedule must be 200 characters or fewer.");

        RuleFor(x => x.EndDate)
            .Must((request, endDate) => endDate is null || endDate >= request.StartDate)
            .WithMessage("End date cannot be before the start date.");

        RuleFor(x => x.MonthlyFeeMinor)
            .GreaterThanOrEqualTo(0).WithMessage("Monthly fee cannot be negative.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must be a three-letter code.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).When(x => x.Capacity is not null)
            .WithMessage("Capacity must be at least 1 when it is set.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must be 1,000 characters or fewer.");
    }
}
