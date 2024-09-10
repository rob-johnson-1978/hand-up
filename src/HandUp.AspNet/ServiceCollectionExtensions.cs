using HandUp.AspNet.ServiceComposition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HandUp.AspNet;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandUpAspNet(this IServiceCollection services)
    {
        services.TryAddScoped<IComposeServicesForMinimalApis, MinimalApiServiceComposer>();

        return services;
    }
}