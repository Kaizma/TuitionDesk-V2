using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuitionDesk.Domain.Classes;

namespace TuitionDesk.Infrastructure.Persistence.Configurations;

public sealed class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClassCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.ClassCode).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(60).IsRequired();
        builder.Property(x => x.GradeLevel).HasMaxLength(40);
        builder.Property(x => x.ScheduleText).HasMaxLength(200);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasMany(x => x.Enrollments)
            .WithOne()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Enrollments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Subject);
        builder.HasIndex(x => x.Status);
    }
}
