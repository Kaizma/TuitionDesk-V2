using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TuitionDesk.Infrastructure.Persistence.Configurations;

public sealed class CounterConfiguration : IEntityTypeConfiguration<Counter>
{
    public void Configure(EntityTypeBuilder<Counter> builder)
    {
        builder.ToTable("Counters");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Key).HasMaxLength(30);
    }
}
