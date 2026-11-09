namespace TuitionDesk.Infrastructure.Persistence;

/// <summary>Sequential code allocator row. Not part of the domain model.</summary>
public sealed class Counter
{
    public string Key { get; set; } = string.Empty;

    public long NextValue { get; set; }
}
