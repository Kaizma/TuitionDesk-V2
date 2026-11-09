using TuitionDesk.Application.Classes;
using TuitionDesk.Server.Http;

namespace TuitionDesk.Server.Endpoints;

public static class ClassEndpoints
{
    public static IEndpointRouteBuilder MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        var classes = app.MapGroup("/api/classes");

        classes.MapGet(string.Empty, async (
            string? search,
            string? status,
            string? subject,
            string? sort,
            int? page,
            int? pageSize,
            Guid? excludeStudentId,
            SearchClassesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new ClassSearch(
                    search,
                    status ?? "active",
                    subject,
                    sort ?? "name",
                    page ?? 1,
                    pageSize ?? 25,
                    excludeStudentId),
                cancellationToken);

            return Results.Ok(result);
        });

        classes.MapGet("/{id:guid}", async (
            Guid id,
            GetClassHandler handler,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await handler.HandleAsync(id, cancellationToken));
        });

        classes.MapPost(string.Empty, async (
                SaveClassRequest request,
                CreateClassHandler handler,
                CancellationToken cancellationToken) =>
            {
                var classEntity = await handler.HandleAsync(request, cancellationToken);
                return Results.Created($"/api/classes/{classEntity.Id}", classEntity);
            })
            .AddEndpointFilter<ValidationFilter<SaveClassRequest>>();

        classes.MapPut("/{id:guid}", async (
                Guid id,
                SaveClassRequest request,
                UpdateClassHandler handler,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await handler.HandleAsync(id, request, cancellationToken));
            })
            .AddEndpointFilter<ValidationFilter<SaveClassRequest>>();

        classes.MapPost("/{id:guid}/archive", async (
            Guid id,
            ArchiveClassHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(id, cancellationToken);
            return Results.NoContent();
        });

        classes.MapPost("/{id:guid}/restore", async (
            Guid id,
            RestoreClassHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(id, cancellationToken);
            return Results.NoContent();
        });

        return app;
    }

    public static IEndpointRouteBuilder MapLookupEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/lookups", async (
            GetLookupsHandler handler,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await handler.HandleAsync(cancellationToken));
        });

        return app;
    }
}
