using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TuitionDesk.Application.Classes;
using TuitionDesk.Application.Enrollments;
using TuitionDesk.Application.Students;

namespace TuitionDesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>(includeInternalTypes: true);

        services.AddScoped<CreateStudentHandler>();
        services.AddScoped<UpdateStudentHandler>();
        services.AddScoped<ArchiveStudentHandler>();
        services.AddScoped<RestoreStudentHandler>();
        services.AddScoped<GetStudentHandler>();
        services.AddScoped<SearchStudentsHandler>();

        services.AddScoped<CreateClassHandler>();
        services.AddScoped<UpdateClassHandler>();
        services.AddScoped<ArchiveClassHandler>();
        services.AddScoped<RestoreClassHandler>();
        services.AddScoped<GetClassHandler>();
        services.AddScoped<SearchClassesHandler>();
        services.AddScoped<GetLookupsHandler>();

        services.AddScoped<EnrollStudentHandler>();
        services.AddScoped<EnrollStudentsHandler>();
        services.AddScoped<WithdrawEnrollmentHandler>();
        services.AddScoped<GetClassRosterHandler>();

        return services;
    }

    /// <summary>Marker type used to locate the application assembly.</summary>
    public sealed class ApplicationAssemblyMarker;
}
