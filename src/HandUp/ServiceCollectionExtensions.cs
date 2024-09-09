using HandUp.ServiceComposition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HandUp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandUp(this IServiceCollection services)
    {
        services.TryAddScoped<IComposeServices, ServiceComposer>();

        return services;
    }
}