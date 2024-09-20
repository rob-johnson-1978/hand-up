using HandUp.ServiceComposition;
using Microsoft.Extensions.DependencyInjection;

namespace HandUp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandUp(this IServiceCollection services, Action<HandUpConfiguration> configurationAction)
    {
        var configuration = new HandUpConfiguration();
        configurationAction.Invoke(configuration);
        services.AddSingleton(configuration);

        foreach (var configurator in configuration.Configurators)
        {
            configurator.AddServices(services);
        }

        var participatorTypes = GetParticipatorTypes(configuration);
        foreach (var (abstraction, implementation) in participatorTypes)
        {
            services.AddScoped(abstraction, implementation);
            configuration.AddImplementor(abstraction, implementation);
        }

        configuration.CheckForDuplicates();

        services.AddScoped<IComposeServices, ServiceComposer>();

        return services;
    }

    private static (Type Abstraction, Type Implementation)[] GetParticipatorTypes(HandUpConfiguration options)
    {
        var assemblies = options.Configurators.Select(configurator => configurator.GetType().Assembly);
        var allTypes = assemblies.SelectMany(ass => ass.GetTypes());

        return allTypes
            .Where(type => 
                type.BaseType is { IsGenericType: true } &&
                type.BaseType.GetGenericTypeDefinition() == typeof(RequestParticipator<,>)
            )
            .Select(type => (type.BaseType!, type))
            .ToArray();
    }
}