using HandUp;
using Microsoft.Extensions.DependencyInjection;

namespace ProductReviewService;

public class ProductReviewServiceHandUpConfigurator : IConfigureHandUp
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        return services;
    }
}