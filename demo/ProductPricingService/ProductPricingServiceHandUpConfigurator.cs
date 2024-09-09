using HandUp;
using Microsoft.Extensions.DependencyInjection;

namespace ProductPricingService;

public class ProductPricingServiceHandUpConfigurator : IConfigureHandUp
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddScoped<IRandomFunction, RandomFunction>();

        return services;
    }
}