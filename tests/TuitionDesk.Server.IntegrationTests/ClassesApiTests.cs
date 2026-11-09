using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TuitionDesk.Application.Classes;
using Xunit;

namespace TuitionDesk.Server.IntegrationTests;

public sealed class ClassesApiTests
{
    [Fact]
    public async Task Create_list_update_and_archive_round_trip()
    {
        await using var app = await TestApp.StartAsync();

        var created = await app.Client.PostAsJsonAsync("/api/classes", SampleClass());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var classEntity = await created.Content.ReadFromJsonAsync<ClassResponse>();
        Assert.NotNull(classEntity);
        Assert.Equal("CLS-0001", classEntity.Code);
        Assert.Equal("active", classEntity.Status);
        Assert.Equal(0, classEntity.ActiveEnrollmentCount);

        var detail = await app.Client.GetFromJsonAsync<ClassResponse>($"/api/classes/{classEntity.Id}");
        Assert.NotNull(detail);
        Assert.Equal("Grade 10 Mathematics", detail.Name);

        var page = await app.Client.GetFromJsonAsync<PagedClasses>("/api/classes?search=Mathematics");
        Assert.NotNull(page);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(400_000, page.Items[0].MonthlyFeeMinor);

        var update = SampleClass() with { Name = "Grade 10 Maths", Capacity = 20 };
        var updated = await app.Client.PutAsJsonAsync($"/api/classes/{classEntity.Id}", update);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var archived = await app.Client.PostAsync($"/api/classes/{classEntity.Id}/archive", null);
        Assert.Equal(HttpStatusCode.NoContent, archived.StatusCode);

        var active = await app.Client.GetFromJsonAsync<PagedClasses>("/api/classes?status=active");
        Assert.NotNull(active);
        Assert.Equal(0, active.TotalCount);

        var archivedList = await app.Client.GetFromJsonAsync<PagedClasses>("/api/classes?status=archived");
        Assert.NotNull(archivedList);
        Assert.Equal(1, archivedList.TotalCount);

        var restored = await app.Client.PostAsync($"/api/classes/{classEntity.Id}/restore", null);
        Assert.Equal(HttpStatusCode.NoContent, restored.StatusCode);

        var after = await app.Client.GetFromJsonAsync<ClassResponse>($"/api/classes/{classEntity.Id}");
        Assert.NotNull(after);
        Assert.Equal("active", after.Status);
    }

    [Fact]
    public async Task Invalid_date_range_returns_validation_error()
    {
        await using var app = await TestApp.StartAsync();

        var request = SampleClass() with
        {
            StartDate = new DateOnly(2027, 1, 1),
            EndDate = new DateOnly(2026, 1, 1),
        };

        var response = await app.Client.PostAsJsonAsync("/api/classes", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("endDate", out _));
    }

    [Fact]
    public async Task Duplicate_class_warns_and_can_be_acknowledged()
    {
        await using var app = await TestApp.StartAsync();

        var first = await app.Client.PostAsJsonAsync("/api/classes", SampleClass());
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await app.Client.PostAsJsonAsync("/api/classes", SampleClass());
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        using var problem = JsonDocument.Parse(await duplicate.Content.ReadAsStringAsync());
        Assert.Equal("class.duplicate_warning", problem.RootElement.GetProperty("code").GetString());

        var acknowledged = await app.Client.PostAsJsonAsync(
            "/api/classes",
            SampleClass() with { AcknowledgeDuplicate = true });
        Assert.Equal(HttpStatusCode.Created, acknowledged.StatusCode);
    }

    [Fact]
    public async Task Lookups_include_existing_subjects_and_grades()
    {
        await using var app = await TestApp.StartAsync();
        await app.Client.PostAsJsonAsync("/api/classes", SampleClass());

        var lookups = await app.Client.GetFromJsonAsync<LookupsResponse>("/api/lookups");
        Assert.NotNull(lookups);
        Assert.Contains("Mathematics", lookups.Subjects);
        Assert.Contains("Grade 10", lookups.GradeLevels);
    }

    private static SaveClassRequest SampleClass()
    {
        return new SaveClassRequest(
            "Grade 10 Mathematics",
            "Mathematics",
            "Grade 10",
            "Wed 4-6pm",
            new DateOnly(2026, 11, 1),
            new DateOnly(2027, 1, 31),
            400_000,
            "LKR",
            15,
            "Bring a calculator.");
    }

    internal sealed record ClassResponse(
        Guid Id,
        string Code,
        string Name,
        string Subject,
        string Status,
        int ActiveEnrollmentCount,
        int? Capacity);

    internal sealed record ClassRow(
        Guid Id,
        string Code,
        string Name,
        string Subject,
        long MonthlyFeeMinor,
        int ActiveEnrollmentCount);

    internal sealed record PagedClasses(IReadOnlyList<ClassRow> Items, int TotalCount);

    internal sealed record LookupsResponse(
        IReadOnlyList<string> Subjects,
        IReadOnlyList<string> GradeLevels);
}
