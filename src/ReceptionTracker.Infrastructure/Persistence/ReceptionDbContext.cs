using Microsoft.EntityFrameworkCore;
using ReceptionTracker.Application.Common;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Infrastructure.Persistence;

public class ReceptionDbContext(DbContextOptions<ReceptionDbContext> options) : DbContext(options), IUnitOfWork
{
    // Only the aggregate root gets a DbSet: pallets, cartons and products are reached through it.
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReceptionDbContext).Assembly);
    }
}
