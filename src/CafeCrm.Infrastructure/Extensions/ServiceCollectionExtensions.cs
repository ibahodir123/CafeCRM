using CafeCrm.Domain.Repositories;
using CafeCrm.Infrastructure.Persistence;
using CafeCrm.Infrastructure.Repositories;
using CafeCrm.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CafeCrm.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=|DataDirectory|\\cafecrm.db";

        services.AddDbContext<CafeCrmDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>();
        services.AddScoped<IEventLogRepository, EventLogRepository>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        return services;
    }
}
