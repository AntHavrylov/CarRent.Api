using CarRent.Application.DataBase;
using CarRent.Application.Repositories;
using CarRent.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CarRent.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<ICarsRepository, CarsRepository>();
        services.AddSingleton<ICarsService, CarsService>();
        services.AddSingleton<IUsersRepository, UsersRepository>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<IOrdersRepository,OrdersRepository>();
        services.AddSingleton<IOrdersService,OrdersService>();
        services.AddSingleton<IRatingsRepository, RatingsRepository>();
        services.AddSingleton<IRatingsService, RatingsService>();
        services.AddValidatorsFromAssemblyContaining<CarRentApplicationMarker>(ServiceLifetime.Singleton);
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString) 
    {
        services.AddSingleton<IDbConnectionFactory>(_ 
            => new NpgsqlConnectionFactory(connectionString));
        services.AddSingleton<DbInitializer>();
        return services;
    }
}
