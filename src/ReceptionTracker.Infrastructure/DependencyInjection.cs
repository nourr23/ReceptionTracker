using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReceptionTracker.Application.Common;
using ReceptionTracker.Application.Orders;
using ReceptionTracker.Infrastructure.Persistence;
using ReceptionTracker.Infrastructure.Persistence.Repositories;
using ReceptionTracker.Infrastructure.Persistence.Seed;

namespace ReceptionTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // The connection string is resolved from the final configuration when the DbContext is created,
        // so hosts (e.g. integration tests) can override it after the services are registered.
        services.AddDbContext<ReceptionDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("ReceptionDb")
                ?? throw new InvalidOperationException("Connection string 'ReceptionDb' is not configured.");

            options
                .UseSqlite(connectionString)
                .UseSeeding((context, _) => OrderSeeder.Seed(context))
                .UseAsyncSeeding((context, _, cancellationToken) => OrderSeeder.SeedAsync(context, cancellationToken));
        });

        // Same scoped DbContext instance: the repository tracks changes, the unit of work commits them.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ReceptionDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }

    /// <summary>Applies pending migrations, then runs the seeding.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReceptionDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
