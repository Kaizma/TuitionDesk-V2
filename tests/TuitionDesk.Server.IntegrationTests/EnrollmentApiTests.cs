using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TuitionDesk.Application.Classes;
using TuitionDesk.Application.Students;
using Xunit;

namespace TuitionDesk.Server.IntegrationTests;

public sealed class EnrollmentApiTests
{
    [Fact]
    public async Task Enrol_duplicate_withdraw_and_counts_round_trip()
    {
        await using var app = await TestApp.StartAsync();
        var student = await CreateStudent(app, "Kasun Perera", "+94 77 123 4567");
        var classEntity = await CreateClass(app);

        var enrolled = await app.Client.PostAsJsonAsync(
            $"/api/classes/{classEntity.Id}/enrollments",
            new { studentId = student.Id });

        Assert.Equal(HttpStatusCode.Created, enrolled.StatusCode);
        var enrollment = await enrolled.Content.ReadFromJsonAsync<EnrollmentResponse>();
        Assert.NotNull(enrollment);
        Assert.Equal("active", enrollment.Status);

        var duplicate = await app.Client.PostAsJsonAsync(
            $"/api/classes/{classEntity.Id}/enrollments",
            new { studentId = student.Id });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        using (var problem = JsonDocument.Parse(await duplicate.Content.ReadAsStringAsync()))
        {
            Assert.Equal("enrollment.already_active", problem.RootElement.GetProperty("code").GetString());
        }

        var roster = await app.Client.GetFromJsonAsync<RosterPage>(
            $"/api/classes/{classEntity.Id}/roster");
        Assert.NotNull(roster);
        Assert.Equal(1, roster.TotalCount);
        Assert.Equal("Kasun Perera", roster.Items[0].FullName);

        var classDetail = await app.Client.GetFromJsonAsync<ClassCountResponse>(
            $"/api/classes/{classEntity.Id}");
        Assert.NotNull(classDetail);
        Assert.Equal(1, classDetail.ActiveEnrollmentCount);

        var studentDetail = await app.Client.GetFromJsonAsync<StudentEnrollmentsResponse>(
            $"/api/students/{student.Id}");
        Assert.NotNull(studentDetail);
        Assert.Single(studentDetail.Enrollments);

        var withdrawn = await app.Client.PostAsync(
            $"/api/enrollments/{enrollment.Id}/withdraw",
            null);
        Assert.Equal(HttpStatusCode.NoContent, withdrawn.StatusCode);

        var activeRoster = await app.Client.GetFromJsonAsync<RosterPage>(
            $"/api/classes/{classEntity.Id}/roster?status=active");
        Assert.NotNull(activeRoster);
        Assert.Equal(0, activeRoster.TotalCount);

        var withdrawnRoster = await app.Client.GetFromJsonAsync<RosterPage>(
            $"/api/classes/{classEntity.Id}/roster?status=withdrawn");
        Assert.NotNull(withdrawnRoster);
        Assert.Equal(1, withdrawnRoster.TotalCount);

        var after = await app.Client.GetFromJsonAsync<ClassCountResponse>(
            $"/api/classes/{classEntity.Id}");
        Assert.NotNull(after);
        Assert.Equal(0, after.ActiveEnrollmentCount);
    }

    [Fact]
    public async Task Bulk_enrol_is_all_or_nothing()
    {
        await using var app = await TestApp.StartAsync();
        var first = await CreateStudent(app, "Amaya Fernando", "+94 76 222 3344");
        var second = await CreateStudent(app, "Dinesh Silva", "+94 71 444 5566");
        var classEntity = await CreateClass(app);

        var bulk = await app.Client.PostAsJsonAsync(
            $"/api/classes/{classEntity.Id}/enrollments/bulk",
            new { studentIds = new[] { first.Id, second.Id } });

        Assert.Equal(HttpStatusCode.OK, bulk.StatusCode);
        using (var document = JsonDocument.Parse(await bulk.Content.ReadAsStringAsync()))
        {
            Assert.Equal(2, document.RootElement.GetProperty("added").GetInt32());
        }

        var duplicate = await app.Client.PostAsJsonAsync(
            $"/api/classes/{classEntity.Id}/enrollments/bulk",
            new { studentIds = new[] { first.Id, second.Id } });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var roster = await app.Client.GetFromJsonAsync<RosterPage>(
            $"/api/classes/{classEntity.Id}/roster");
        Assert.NotNull(roster);
        Assert.Equal(2, roster.TotalCount);
    }

    [Fact]
    public async Task Archiving_a_student_withdraws_active_enrolments()
    {
        await using var app = await TestApp.StartAsync();
        var student = await CreateStudent(app, "Nadeesha Jayasuriya", "+94 70 777 8899");
        var classEntity = await CreateClass(app);

        await app.Client.PostAsJsonAsync(
            $"/api/classes/{classEntity.Id}/enrollments",
            new { studentId = student.Id });

        var archived = await app.Client.PostAsync($"/api/students/{student.Id}/archive", null);
        Assert.Equal(HttpStatusCode.NoContent, archived.StatusCode);

        var classDetail = await app.Client.GetFromJsonAsync<ClassCountResponse>(
            $"/api/classes/{classEntity.Id}");
        Assert.NotNull(classDetail);
        Assert.Equal(0, classDetail.ActiveEnrollmentCount);

        var withdrawn = await app.Client.GetFromJsonAsync<RosterPage>(
            $"/api/classes/{classEntity.Id}/roster?status=withdrawn");
        Assert.NotNull(withdrawn);
        Assert.Equal(1, withdrawn.TotalCount);
    }

    private static async Task<StudentResponse> CreateStudent(TestApp app, string name, string phone)
    {
        var response = await app.Client.PostAsJsonAsync("/api/students", new SaveStudentRequest(
            name,
            phone,
            null,
            null,
            null,
            null,
            null,
            null,
            new DateOnly(2026, 1, 10),
            null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var student = await response.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(student);
        return student;
    }

    private static async Task<ClassResponse> CreateClass(TestApp app)
    {
        var response = await app.Client.PostAsJsonAsync("/api/classes", new SaveClassRequest(
            "Grade 10 Mathematics",
            "Mathematics",
            "Grade 10",
            "Wed 4-6pm",
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 12, 10),
            400_000,
            "LKR",
            15,
            null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var classEntity = await response.Content.ReadFromJsonAsync<ClassResponse>();
        Assert.NotNull(classEntity);
        return classEntity;
    }

    internal sealed record StudentResponse(Guid Id, string Code, string FullName);
    internal sealed record ClassResponse(Guid Id, string Code, string Name);

    internal sealed record ClassCountResponse(Guid Id, int ActiveEnrollmentCount);

    internal sealed record StudentEnrollmentsResponse(
        Guid Id,
        IReadOnlyList<EnrollmentResponse> Enrollments);

    internal sealed record EnrollmentResponse(
        Guid Id,
        Guid StudentId,
        Guid ClassId,
        DateOnly EnrolledOn,
        string Status);

    internal sealed record RosterRow(
        Guid EnrollmentId,
        Guid StudentId,
        string FullName,
        string Status,
        string StudentStatus);

    internal sealed record RosterPage(IReadOnlyList<RosterRow> Items, int TotalCount);
}
