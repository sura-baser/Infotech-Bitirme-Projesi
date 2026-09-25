using Microsoft.Extensions.DependencyInjection;
using PastaneApp.Core.Services;

namespace PastaneApp.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPartyBoxService, PartyBoxService>();
        return services;
    }
}
