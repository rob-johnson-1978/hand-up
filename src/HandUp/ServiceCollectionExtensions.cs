using System.Reflection;
using HandUp.ServiceComposition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HandUp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandUp(this IServiceCollection services, Action<HandUpOptions> optionsAction)
    {
        var options = new HandUpOptions();
        optionsAction.Invoke(options);

        foreach (var configureHandUp in options.Configurators)
        {
            configureHandUp.AddServices(services);
        }

        var participatorTypes = GetParticipatorTypes(options);
        foreach (var participatorType in participatorTypes)
        {
            services.AddScoped(participatorType.Interface, participatorType.Implmentation);
        }

        services.AddScoped<IComposeServices, ServiceComposer>();

        return services;
    }

    private static (Type Interface, Type Implmentation)[] GetParticipatorTypes(HandUpOptions options)
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