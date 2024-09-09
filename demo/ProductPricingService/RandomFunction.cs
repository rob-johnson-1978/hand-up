namespace ProductPricingService;

public class RandomFunction : IRandomFunction
{
    public void DoSomethingRandom()
    {
        var xyz = Guid.NewGuid();

        if (xyz == Guid.Empty)
        {
            throw new Exception("Oops");
        }
    }
}