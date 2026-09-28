using Microsoft.Extensions.DependencyInjection;
using ReceptionTracker.Application.Orders;

namespace ReceptionTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IReceptionService, ReceptionService>();

        return services;
    }
}
