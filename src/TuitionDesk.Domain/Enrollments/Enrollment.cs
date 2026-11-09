using TuitionDesk.Domain.Common;

namespace TuitionDesk.Domain.Enrollments;

public sealed class Enrollment
{
    private Enrollment()
    {
    }

    public Guid Id { get; private set; }

    public Guid StudentId { get; private set; }

    public Guid ClassId { get; private set; }

    public DateOnly EnrolledOn { get; private set; }

    public EnrollmentStatus Status { get; private set; }

    public DateOnly? WithdrawnOn { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Enrollment Create(
        Guid studentId,
        Guid classId,
        DateOnly enrolledOn,
        DateOnly today,
        DateTimeOffset now)
    {
        if (studentId == Guid.Empty)
        {
            throw new DomainValidationException("enrollment.invalid_student", "A student is required.");
        }

        if (classId == Guid.Empty)
        {
            throw new DomainValidationException("enrollment.invalid_class", "A class is required.");
        }

        if (enrolledOn > today)
        {
            throw new DomainValidationException(
                "enrollment.invalid_date",
                "Enrolment date cannot be in the future.");
        }

        return new Enrollment
        {
            Id = Guid.CreateVersion7(),
            StudentId = studentId,
            ClassId = classId,
            EnrolledOn = enrolledOn,
            Status = EnrollmentStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }

    public void Withdraw(DateOnly today, DateTimeOffset now)
    {
        if (Status == EnrollmentStatus.Withdrawn)
        {
            throw new DomainStateException(
                "enrollment.already_withdrawn",
                "This enrolment has already been withdrawn.");
        }

        if (today < EnrolledOn)
        {
            throw new DomainValidationException(
                "enrollment.invalid_withdrawal_date",
                "Withdrawal date cannot be before the enrolment date.");
        }

        Status = EnrollmentStatus.Withdrawn;
        WithdrawnOn = today;
        UpdatedAtUtc = now;
    }
}
