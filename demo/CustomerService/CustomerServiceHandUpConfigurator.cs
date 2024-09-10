using HandUp;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService;

public class CustomerServiceHandUpConfigurator : IConfigureHandUp
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        return services;
    }
}