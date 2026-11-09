using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuitionDesk.Domain.Enrollments;
using TuitionDesk.Domain.Students;

namespace TuitionDesk.Infrastructure.Persistence.Configurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // One active enrolment per student/class pair; withdrawn history is unconstrained.
        builder.HasIndex(x => new { x.StudentId, x.ClassId })
            .HasFilter("\"Status\" = 0")
            .IsUnique();

        builder.HasIndex(x => new { x.ClassId, x.Status });
        builder.HasIndex(x => new { x.StudentId, x.Status });
    }
}
