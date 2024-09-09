using Contracts;
using HandUp;

namespace ProductPricingService;

public class ProductByIdRequestParticipator(IRandomFunction randomFunction) : IParticipateInRequests<ProductByIdRequest, ProductByIdResponse>
{
    public bool Ready(ProductByIdResponse response) => true; // always ready

    public async Task ParticipateAsync(ProductByIdRequest request, ProductByIdResponse response)
    {
        await Task.CompletedTask;
        
        randomFunction.DoSomethingRandom();

        response.CurrentPrice = 12.12M;
    }
}