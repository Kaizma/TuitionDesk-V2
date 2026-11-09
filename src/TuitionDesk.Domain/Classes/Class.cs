using TuitionDesk.Domain.Common;
using TuitionDesk.Domain.Enrollments;

namespace TuitionDesk.Domain.Classes;

public sealed class Class
{
    private readonly List<Enrollment> _enrollments = [];

    private Class()
    {
    }

    public Guid Id { get; private set; }

    public string ClassCode { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string? GradeLevel { get; private set; }

    public string? ScheduleText { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public long MonthlyFeeMinor { get; private set; }

    public string Currency { get; private set; } = "LKR";

    public int? Capacity { get; private set; }

    public string? Notes { get; private set; }

    public ClassStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments;

    public static Class Create(string classCode, ClassDetails details, DateTimeOffset now)
    {
        var entity = new Class
        {
            Id = Guid.CreateVersion7(),
            ClassCode = ClassCodes.Normalize(classCode),
            Status = ClassStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        entity.Apply(details);
        return entity;
    }

    public void Update(ClassDetails details, DateTimeOffset now)
    {
        EnsureActive();
        Apply(details);
        UpdatedAtUtc = now;
    }

    public void Archive(DateTimeOffset now)
    {
        if (Status == ClassStatus.Archived)
        {
            throw new DomainStateException("class.already_archived", "This class is already archived.");
        }

        Status = ClassStatus.Archived;
        UpdatedAtUtc = now;
    }

    public void Restore(DateTimeOffset now)
    {
        if (Status == ClassStatus.Active)
        {
            throw new DomainStateException("class.already_active", "This class is already active.");
        }

        Status = ClassStatus.Active;
        UpdatedAtUtc = now;
    }

    public void EnsureActive()
    {
        if (Status == ClassStatus.Archived)
        {
            throw new DomainStateException(
                "class.archived",
                "This class is archived. Restore it before making changes.");
        }
    }

    public void EnsureCanEnroll()
    {
        EnsureActive();
    }

    public void AddEnrollment(Enrollment enrollment)
    {
        ArgumentNullException.ThrowIfNull(enrollment);

        if (enrollment.ClassId != Id)
        {
            throw new ArgumentException("Enrolment does not belong to this class.", nameof(enrollment));
        }

        if (_enrollments.Any(e =>
                e.StudentId == enrollment.StudentId &&
                e.Status == EnrollmentStatus.Active))
        {
            throw new DomainStateException(
                "enrollment.already_active",
                "This student is already enrolled in this class.");
        }

        _enrollments.Add(enrollment);
    }

    private void Apply(ClassDetails details)
    {
        Name = Guard.RequiredName(details.Name, "class.invalid_name", "Class name", 2, 120);
        Subject = Guard.RequiredText(details.Subject, "class.invalid_subject", "Subject", 2, 60);
        GradeLevel = Guard.OptionalText(details.GradeLevel, "class.invalid_grade", "Grade level", 40);
        ScheduleText = Guard.OptionalText(details.ScheduleText, "class.invalid_schedule", "Schedule", 200);
        StartDate = details.StartDate;

        if (details.EndDate is { } end && end < details.StartDate)
        {
            throw new DomainValidationException(
                "class.invalid_end_date",
                "End date cannot be before the start date.");
        }

        EndDate = details.EndDate;
        MonthlyFeeMinor = Guard.NonNegative(details.MonthlyFeeMinor, "class.invalid_fee", "Monthly fee");
        Currency = Guard.RequiredCurrency(details.Currency, "class.invalid_currency");
        Capacity = Guard.PositiveCapacity(details.Capacity, "class.invalid_capacity");
        Notes = Guard.OptionalText(details.Notes, "class.invalid_notes", "Notes", 1000);
    }
}
