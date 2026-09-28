using Microsoft.EntityFrameworkCore;
using ReceptionTracker.Application.Orders;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(ReceptionDbContext dbContext) : IOrderRepository
{
    public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default) =>
        await WithHierarchy()
            .AsNoTracking()
            .OrderBy(o => o.Reference)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default) =>
        WithHierarchy().SingleOrDefaultAsync(o => o.Reference == reference, cancellationToken);

    // One SQL query per level instead of one big JOIN that duplicates every order/pallet row per product.
    private IQueryable<Order> WithHierarchy() =>
        dbContext.Orders
            .Include(o => o.Pallets)
                .ThenInclude(p => p.Cartons)
                    .ThenInclude(c => c.Products)
            .AsSplitQuery();
}
