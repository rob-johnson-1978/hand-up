namespace HandUp;

public class HandUpConfiguration
{
    public int MaxParticipationLoopCount { get; set; } = 10;

    public HandUpConfiguration AddConfigurator(IConfigureHandUp configurator)
    {
        Configurators.Add(configurator);
        return this;
    }

    internal readonly List<IConfigureHandUp> Configurators = [];

    internal readonly Dictionary<Type, List<Type>> ImplementorSets = [];

    internal void AddImplementor(Type abstraction, Type implementation)
    {
        if (ImplementorSets.TryGetValue(abstraction, out var implementors))
        {
            implementors.Add(implementation);
            return;
        }

        ImplementorSets[abstraction] = [implementation];
    }

    internal void CheckForDuplicates()
    {
        foreach (var implementorSet in ImplementorSets)
        {
            var uniqueCount = implementorSet.Value.Select(x => x.ToString()).Count();
            var actualCount = implementorSet.Value.Count;

            if (uniqueCount < actualCount)
            {
                throw new Exception(
                    $"There are duplicated implementors against {implementorSet.Key}. "
                    + $"The values are {string.Join(", ", implementorSet.Value.Select(x => x.ToString()))}"
                );
            }
        }
    }
}