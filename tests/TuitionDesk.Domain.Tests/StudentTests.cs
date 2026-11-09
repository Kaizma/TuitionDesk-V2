using TuitionDesk.Domain.Classes;
using TuitionDesk.Domain.Common;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Domain.Students;
using Xunit;

namespace TuitionDesk.Domain.Tests;

public sealed class StudentTests
{
    private static readonly DateOnly Today = new(2026, 11, 8);
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_trims_values_and_starts_active()
    {
        var student = Student.Create(" stu-00001 ", Details(fullName: "  Kasun Perera  "), Today, Now);

        Assert.Equal("STU-00001", student.StudentCode);
        Assert.Equal("Kasun Perera", student.FullName);
        Assert.Equal("+94 77 123 4567", student.Phone);
        Assert.Equal(StudentStatus.Active, student.Status);
        Assert.NotEqual(Guid.Empty, student.Id);
        Assert.Equal(Now, student.CreatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("12345")]
    public void Create_rejects_invalid_names(string name)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Student.Create("STU-00001", Details(fullName: name), Today, Now));

        Assert.Equal("student.invalid_name", exception.Code);
    }

    [Fact]
    public void Create_rejects_invalid_phone()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Student.Create("STU-00001", Details(phone: "call me"), Today, Now));

        Assert.Equal("student.invalid_phone", exception.Code);
    }

    [Fact]
    public void Create_rejects_future_joined_date()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => Student.Create("STU-00001", Details(joinedOn: Today.AddDays(1)), Today, Now));

        Assert.Equal("student.invalid_joined_on", exception.Code);
    }

    [Fact]
    public void Archived_students_are_read_only_until_restored()
    {
        var student = Student.Create("STU-00001", Details(), Today, Now);

        student.Archive(Now);

        Assert.Equal(StudentStatus.Archived, student.Status);
        var exception = Assert.Throws<DomainStateException>(
            () => student.Update(Details(fullName: "New Name"), Today, Now));
        Assert.Equal("student.archived", exception.Code);

        student.Restore(Now.AddMinutes(1));
        Assert.Equal(StudentStatus.Active, student.Status);

        student.Update(Details(fullName: "New Name"), Today, Now);
        Assert.Equal("New Name", student.FullName);
    }

    [Fact]
    public void Cannot_archive_twice()
    {
        var student = Student.Create("STU-00001", Details(), Today, Now);
        student.Archive(Now);

        var exception = Assert.Throws<DomainStateException>(() => student.Archive(Now));

        Assert.Equal("student.already_archived", exception.Code);
    }

    private static StudentDetails Details(
        string fullName = "Kasun Perera",
        string phone = "+94 77 123 4567",
        DateOnly? joinedOn = null)
    {
        return new StudentDetails(
            fullName,
            phone,
            "kasun@example.com",
            "Nimal Perera",
            "+94 71 555 1234",
            new DateOnly(2009, 4, 18),
            "Ananda College",
            "Grade 10",
            joinedOn ?? Today,
            "Notes");
    }
}

public sealed class ClassAndEnrollmentTests
{
    private static readonly DateOnly Today = new(2026, 11, 8);
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Class_rejects_end_date_before_start()
    {
        var details = CreateClassDetails() with { StartDate = Today, EndDate = Today.AddDays(-1) };

        var exception = Assert.Throws<DomainValidationException>(
            () => Class.Create("CLS-0001", details, Now));

        Assert.Equal("class.invalid_end_date", exception.Code);
    }

    [Fact]
    public void Class_rejects_zero_capacity()
    {
        var details = CreateClassDetails() with { Capacity = 0 };

        var exception = Assert.Throws<DomainValidationException>(
            () => Class.Create("CLS-0001", details, Now));

        Assert.Equal("class.invalid_capacity", exception.Code);
    }

    [Fact]
    public void Class_rejects_duplicate_active_enrollment()
    {
        var classEntity = Class.Create("CLS-0001", CreateClassDetails(), Now);
        var first = Enrollment.Create(Guid.CreateVersion7(), classEntity.Id, Today, Today, Now);

        classEntity.AddEnrollment(first);

        var second = Enrollment.Create(first.StudentId, classEntity.Id, Today, Today, Now);
        var exception = Assert.Throws<DomainStateException>(() => classEntity.AddEnrollment(second));
        Assert.Equal("enrollment.already_active", exception.Code);
    }

    [Fact]
    public void Enrollment_rejects_future_date_and_double_withdrawal()
    {
        var future = Assert.Throws<DomainValidationException>(
            () => Enrollment.Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Today.AddDays(1),
                Today,
                Now));
        Assert.Equal("enrollment.invalid_date", future.Code);

        var enrollment = Enrollment.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), Today, Today, Now);
        enrollment.Withdraw(Today, Now);

        var second = Assert.Throws<DomainStateException>(() => enrollment.Withdraw(Today, Now));
        Assert.Equal("enrollment.already_withdrawn", second.Code);
    }

    private static ClassDetails CreateClassDetails()
    {
        return new ClassDetails(
            "Grade 10 Mathematics",
            "Mathematics",
            "Grade 10",
            "Wed 4-6pm",
            Today,
            Today.AddMonths(3),
            400_000,
            "LKR",
            15,
            null);
    }
}
