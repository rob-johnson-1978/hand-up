using HandUp;
using Microsoft.Extensions.DependencyInjection;

namespace SalesService;

public class SalesServiceHandUpConfigurator : IConfigureHandUp
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        return services;
    }
}