using HandUp;
using Microsoft.Extensions.DependencyInjection;

namespace ProductDetailsService;

public class ProductDetailsServiceHandUpConfigurator : IConfigureHandUp
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        return services;
    }
}