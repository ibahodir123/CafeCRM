using CafeCrm.Application.Abstractions;
using CafeCrm.Application.Contracts;
using CafeCrm.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CafeCrm.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<CustomerService>();
        services.AddScoped<ReservationService>();
        services.AddScoped<VisitService>();
        services.AddScoped<LoyaltyService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IEventHistoryService, EventHistoryService>();
        services.AddHostedService<PosTicketIngestionService>();
        return services;
    }
}
