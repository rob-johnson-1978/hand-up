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
        foreach (var participatorType in participatorTypes)
        {
            services.AddScoped(participatorType.Interface, participatorType.Implmentation);
        }

        services.AddScoped<IComposeServices, ServiceComposer>();

        return services;
    }

    private static (Type Interface, Type Implmentation)[] GetParticipatorTypes(HandUpConfiguration options)
    {
        var assemblies = options.Configurators.Select(configurator => configurator.GetType().Assembly);
        var allTypes = assemblies.SelectMany(ass => ass.GetTypes());

        return allTypes
            .Where(
                type =>
                {
                    var interfaces = type.GetInterfaces().Where(iface =>
                        iface is { IsGenericType: true, GenericTypeArguments.Length: 2 } &&
                        iface.GetGenericTypeDefinition() == typeof(IParticipateInRequests<,>)
                    ).ToArray();

                    return interfaces.Length switch
                    {
                        < 1 => false,
                        > 1 => throw new Exception($"An implementation of {typeof(IParticipateInRequests<,>).Name} cannot have more than one interface"),
                        _ => true
                    };
                })
            .Select(type => (type.GetInterfaces().Single(), type))
            .ToArray();
    }
}