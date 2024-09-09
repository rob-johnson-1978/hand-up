using Microsoft.Extensions.DependencyInjection;

namespace HandUp;

public interface IConfigureHandUp
{
    IServiceCollection AddServices(IServiceCollection services);
}