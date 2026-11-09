using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuitionDesk.Domain.Students;

namespace TuitionDesk.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.StudentCode).IsUnique();

        builder.Property(x => x.FullName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.GuardianName).HasMaxLength(120);
        builder.Property(x => x.GuardianPhone).HasMaxLength(20);
        builder.Property(x => x.School).HasMaxLength(120);
        builder.Property(x => x.GradeLevel).HasMaxLength(40);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasIndex(x => x.FullName);
        builder.HasIndex(x => x.Phone);
        builder.HasIndex(x => x.Status);
    }
}
