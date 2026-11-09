using TuitionDesk.Application.Students;
using TuitionDesk.Server.Http;

namespace TuitionDesk.Server.Endpoints;

public static class StudentEndpoints
{
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        var students = app.MapGroup("/api/students");

        students.MapGet(string.Empty, async (
            string? search,
            string? status,
            string? sort,
            int? page,
            int? pageSize,
            Guid? excludeClassId,
            SearchStudentsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new StudentSearch(
                    search,
                    status ?? "active",
                    sort ?? "name",
                    page ?? 1,
                    pageSize ?? 25,
                    excludeClassId),
                cancellationToken);

            return Results.Ok(result);
        });

        students.MapGet("/{id:guid}", async (
            Guid id,
            GetStudentHandler handler,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await handler.HandleAsync(id, cancellationToken));
        });

        students.MapPost(string.Empty, async (
                SaveStudentRequest request,
                CreateStudentHandler handler,
                CancellationToken cancellationToken) =>
            {
                var student = await handler.HandleAsync(request, cancellationToken);
                return Results.Created($"/api/students/{student.Id}", student);
            })
            .AddEndpointFilter<ValidationFilter<SaveStudentRequest>>();

        students.MapPut("/{id:guid}", async (
                Guid id,
                SaveStudentRequest request,
                UpdateStudentHandler handler,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await handler.HandleAsync(id, request, cancellationToken));
            })
            .AddEndpointFilter<ValidationFilter<SaveStudentRequest>>();

        students.MapPost("/{id:guid}/archive", async (
            Guid id,
            ArchiveStudentHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(id, cancellationToken);
            return Results.NoContent();
        });

        students.MapPost("/{id:guid}/restore", async (
            Guid id,
            RestoreStudentHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(id, cancellationToken);
            return Results.NoContent();
        });

        return app;
    }
}
