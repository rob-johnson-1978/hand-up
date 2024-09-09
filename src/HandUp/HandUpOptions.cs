namespace HandUp;

public class HandUpOptions
{
    internal readonly List<IConfigureHandUp> Configurators = [];

    public HandUpOptions AddConfigurator(IConfigureHandUp configurator)
    {
        Configurators.Add(configurator);
        return this;
    }
}