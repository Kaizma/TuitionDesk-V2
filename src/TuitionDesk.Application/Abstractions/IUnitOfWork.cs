namespace TuitionDesk.Application.Abstractions;

/// <summary>Commits all pending changes for the current use case as one transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
