using Contracts;
using HandUp;

namespace ProductPricingService;

public class ProductByIdRequestParticipator(IRandomFunction randomFunction) : RequestParticipator<ProductByIdRequest, ProductByIdResponse>
{
    public override async Task ParticipateAsync(ProductByIdRequest request, ComposeResult<ProductByIdResponse> ongoingComposeResult)
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