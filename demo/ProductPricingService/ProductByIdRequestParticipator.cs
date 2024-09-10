using Contracts;
using HandUp;

namespace ProductPricingService;

public class ProductByIdRequestParticipator(IRandomFunction randomFunction) : IParticipateInRequests<ProductByIdRequest, ProductByIdResponse>
{
    public bool WillPopulateCollectionSkeleton => false;

    public bool Ready(ComposeResult<ProductByIdResponse> ongoingComposeResult) => true; // always ready

    public async Task ParticipateAsync(ProductByIdRequest request, ComposeResult<ProductByIdResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (request.ProductId != 123)
        {
            ongoingComposeResult.NotFoundOrNoResults = true;
            return;
        }

        randomFunction.DoSomethingRandom();

        ongoingComposeResult.Response.CurrentPrice = 12.12M;
    }
}