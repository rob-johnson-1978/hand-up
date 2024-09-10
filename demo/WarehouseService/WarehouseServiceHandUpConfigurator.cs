using HandUp;
using Microsoft.Extensions.DependencyInjection;

namespace WarehouseService;

public class WarehouseServiceHandUpConfigurator : IConfigureHandUp
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        return services;
    }
}