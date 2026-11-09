using TuitionDesk.Domain.Common;

namespace TuitionDesk.Domain.Students;

public sealed class Student
{
    private Student()
    {
    }

    public Guid Id { get; private set; }

    public string StudentCode { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? GuardianName { get; private set; }

    public string? GuardianPhone { get; private set; }

    public DateOnly? DateOfBirth { get; private set; }

    public string? School { get; private set; }

    public string? GradeLevel { get; private set; }

    public DateOnly JoinedOn { get; private set; }

    public string? Notes { get; private set; }

    public StudentStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Student Create(
        string studentCode,
        StudentDetails details,
        DateOnly today,
        DateTimeOffset now)
    {
        var student = new Student
        {
            Id = Guid.CreateVersion7(),
            StudentCode = StudentCodes.Normalize(studentCode),
            Status = StudentStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        student.Apply(details, today);
        return student;
    }

    public void Update(StudentDetails details, DateOnly today, DateTimeOffset now)
    {
        EnsureActive();
        Apply(details, today);
        UpdatedAtUtc = now;
    }

    public void Archive(DateTimeOffset now)
    {
        if (Status == StudentStatus.Archived)
        {
            throw new DomainStateException("student.already_archived", "This student is already archived.");
        }

        Status = StudentStatus.Archived;
        UpdatedAtUtc = now;
    }

    public void Restore(DateTimeOffset now)
    {
        if (Status == StudentStatus.Active)
        {
            throw new DomainStateException("student.already_active", "This student is already active.");
        }

        Status = StudentStatus.Active;
        UpdatedAtUtc = now;
    }

    public void EnsureActive()
    {
        if (Status == StudentStatus.Archived)
        {
            throw new DomainStateException(
                "student.archived",
                "This student is archived. Restore them before making changes.");
        }
    }

    private void Apply(StudentDetails details, DateOnly today)
    {
        FullName = Guard.RequiredName(details.FullName, "student.invalid_name", "Full name", 2, 120);
        Phone = Guard.RequiredPhone(details.Phone, "student.invalid_phone", "Phone");
        Email = Guard.OptionalEmail(details.Email, "student.invalid_email", "Email");
        GuardianName = Guard.OptionalText(details.GuardianName, "student.invalid_guardian_name", "Guardian name", 120);
        GuardianPhone = Guard.OptionalPhone(details.GuardianPhone, "student.invalid_guardian_phone", "Guardian phone");
        DateOfBirth = Guard.NotInFuture(details.DateOfBirth, today, "student.invalid_date_of_birth", "Date of birth");
        School = Guard.OptionalText(details.School, "student.invalid_school", "School", 120);
        GradeLevel = Guard.OptionalText(details.GradeLevel, "student.invalid_grade", "Grade level", 40);
        JoinedOn = Guard.NotInFuture(details.JoinedOn, today, "student.invalid_joined_on", "Joined date");
        Notes = Guard.OptionalText(details.Notes, "student.invalid_notes", "Notes", 1000);
    }
}
