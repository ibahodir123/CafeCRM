using CafeCrm.Application.Contracts;
using CafeCrm.Pos.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CafeCrm.Pos;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPosAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PosAdapterOptions>(configuration.GetSection("PosAdapter"));
        services.AddSingleton<IPosClient, MockPosClient>();
        return services;
    }
}
