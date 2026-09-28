namespace ReceptionTracker.Application.Common;

/// <summary>Commits all the changes made during a use case in a single transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
