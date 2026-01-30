using Books.Application.Repositories;
using Books.Application.Services;
using Books.Infrastructure.Repositories;
using Books.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Books.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IBookRepository, InMemoryBookRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
