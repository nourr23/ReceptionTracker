using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Application.Orders;

public interface IOrderRepository
{
    /// <summary>All orders with their full hierarchy, read-only.</summary>
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>One order with its full hierarchy, tracked so changes can be saved.</summary>
    Task<Order?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
}
