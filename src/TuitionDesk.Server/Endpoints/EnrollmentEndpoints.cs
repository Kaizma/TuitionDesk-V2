using TuitionDesk.Application.Enrollments;
using TuitionDesk.Server.Http;

namespace TuitionDesk.Server.Endpoints;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        var classes = app.MapGroup("/api/classes");

        classes.MapGet("/{id:guid}/roster", async (
            Guid id,
            string? search,
            string? status,
            int? page,
            int? pageSize,
            GetClassRosterHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new RosterSearch(
                    id,
                    search,
                    status ?? "active",
                    page ?? 1,
                    pageSize ?? 25),
                cancellationToken);

            return Results.Ok(result);
        });

        classes.MapPost("/{id:guid}/enrollments", async (
                Guid id,
                EnrollStudentRequest request,
                EnrollStudentHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, request, cancellationToken);
                return Results.Created($"/api/classes/{id}/roster", result);
            })
            .AddEndpointFilter<ValidationFilter<EnrollStudentRequest>>();

        classes.MapPost("/{id:guid}/enrollments/bulk", async (
                Guid id,
                EnrollStudentsRequest request,
                EnrollStudentsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var added = await handler.HandleAsync(id, request, cancellationToken);
                return Results.Ok(new { added });
            })
            .AddEndpointFilter<ValidationFilter<EnrollStudentsRequest>>();

        app.MapPost("/api/enrollments/{id:guid}/withdraw", async (
            Guid id,
            WithdrawEnrollmentHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(id, cancellationToken);
            return Results.NoContent();
        });

        return app;
    }
}
