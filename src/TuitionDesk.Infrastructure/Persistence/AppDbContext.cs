using Microsoft.EntityFrameworkCore;
using TuitionDesk.Domain.Classes;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Domain.Students;

namespace TuitionDesk.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Class> Classes => Set<Class>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Counter> Counters => Set<Counter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
