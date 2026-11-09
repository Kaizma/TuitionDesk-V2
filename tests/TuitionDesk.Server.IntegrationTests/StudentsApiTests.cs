using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TuitionDesk.Application.Students;
using TuitionDesk.Server.Hosting;
using Xunit;

namespace TuitionDesk.Server.IntegrationTests;

public sealed class StudentsApiTests
{
    [Fact]
    public async Task Create_read_update_and_search_round_trip()
    {
        await using var app = await TestApp.StartAsync();

        var created = await app.Client.PostAsJsonAsync("/api/students", SampleRequest());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var student = await created.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(student);
        Assert.Equal("STU-00001", student.Code);
        Assert.Equal("active", student.Status);

        var detail = await app.Client.GetFromJsonAsync<StudentResponse>($"/api/students/{student.Id}");
        Assert.NotNull(detail);
        Assert.Equal("Kasun Perera", detail.FullName);

        var update = SampleRequest() with { FullName = "Kasun P. Perera" };
        var updated = await app.Client.PutAsJsonAsync($"/api/students/{student.Id}", update);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var page = await app.Client.GetFromJsonAsync<PagedResponse<StudentRow>>("/api/students?search=Kasun");
        Assert.NotNull(page);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal("Kasun P. Perera", page.Items[0].FullName);
    }

    [Fact]
    public async Task Invalid_request_returns_field_errors()
    {
        await using var app = await TestApp.StartAsync();

        var response = await app.Client.PostAsJsonAsync("/api/students", SampleRequest() with
        {
            FullName = "",
            Phone = "12",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("fullName", out _));
        Assert.True(errors.TryGetProperty("phone", out _));
    }

    [Fact]
    public async Task Duplicate_name_and_phone_warns_then_allows_acknowledgement()
    {
        await using var app = await TestApp.StartAsync();
        var request = SampleRequest();

        var first = await app.Client.PostAsJsonAsync("/api/students", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await app.Client.PostAsJsonAsync("/api/students", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        using var problem = JsonDocument.Parse(await duplicate.Content.ReadAsStringAsync());
        Assert.Equal(
            "student.duplicate_warning",
            problem.RootElement.GetProperty("code").GetString());

        var acknowledged = await app.Client.PostAsJsonAsync(
            "/api/students",
            request with { AcknowledgeDuplicate = true });
        Assert.Equal(HttpStatusCode.Created, acknowledged.StatusCode);
    }

    [Fact]
    public async Task Archive_and_restore_move_student_between_filters()
    {
        await using var app = await TestApp.StartAsync();
        var created = await app.Client.PostAsJsonAsync("/api/students", SampleRequest());
        var student = await created.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(student);

        var archived = await app.Client.PostAsync($"/api/students/{student.Id}/archive", null);
        Assert.Equal(HttpStatusCode.NoContent, archived.StatusCode);

        var activePage = await app.Client.GetFromJsonAsync<PagedResponse<StudentRow>>("/api/students?status=active");
        Assert.NotNull(activePage);
        Assert.Equal(0, activePage.TotalCount);

        var archivedPage = await app.Client.GetFromJsonAsync<PagedResponse<StudentRow>>("/api/students?status=archived");
        Assert.NotNull(archivedPage);
        Assert.Equal(1, archivedPage.TotalCount);

        var restored = await app.Client.PostAsync($"/api/students/{student.Id}/restore", null);
        Assert.Equal(HttpStatusCode.NoContent, restored.StatusCode);

        var detail = await app.Client.GetFromJsonAsync<StudentResponse>($"/api/students/{student.Id}");
        Assert.NotNull(detail);
        Assert.Equal("active", detail.Status);
    }

    internal static SaveStudentRequest SampleRequest()
    {
        return new SaveStudentRequest(
            "Kasun Perera",
            "+94 77 123 4567",
            "kasun@example.com",
            "Nimal Perera",
            "+94 71 555 1234",
            new DateOnly(2009, 4, 18),
            "Ananda College",
            "Grade 10",
            new DateOnly(2026, 11, 8),
            "Notes");
    }

    internal sealed record StudentResponse(
        Guid Id,
        string Code,
        string FullName,
        string Status);

    internal sealed record StudentRow(
        Guid Id,
        string Code,
        string FullName,
        string Status,
        int ActiveClassCount);

    internal sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}

internal sealed class TestApp : IAsyncDisposable
{
    private readonly string _dataDirectory;

    private TestApp(string dataDirectory, ServerInstance server, HttpClient client)
    {
        _dataDirectory = dataDirectory;
        Server = server;
        Client = client;
    }

    public ServerInstance Server { get; }

    public HttpClient Client { get; }

    public static async Task<TestApp> StartAsync(bool requireToken = false)
    {
        var dataDirectory = Path.Combine(
            Path.GetTempPath(),
            "tuitiondesk-integration",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(dataDirectory);

        var server = await ServerHost.StartAsync(new ServerOptions
        {
            DataDirectory = dataDirectory,
            Urls = ["http://127.0.0.1:0"],
            RequireToken = requireToken,
            ContentRootPath = AppContext.BaseDirectory,
            ServeSpa = false,
            EnvironmentName = "Development",
        });

        var client = new HttpClient { BaseAddress = new Uri(server.Url) };
        return new TestApp(dataDirectory, server, client);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await Server.DisposeAsync();

        try
        {
            if (Directory.Exists(_dataDirectory))
            {
                Directory.Delete(_dataDirectory, recursive: true);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // SQLite may still hold the file briefly; temp cleanup is best effort.
        }
    }
}
