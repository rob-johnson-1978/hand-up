namespace HandUp;

public class HandUpConfiguration
{
    internal readonly List<IConfigureHandUp> Configurators = [];
   
    public int MaxParticipationLoopCount { get; set; } = 10;

    public HandUpConfiguration AddConfigurator(IConfigureHandUp configurator)
    {
        Configurators.Add(configurator);
        return this;
    }
}