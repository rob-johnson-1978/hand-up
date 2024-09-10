using Contracts;
using HandUp;

namespace ProductDetailsService;

public class ProductByIdRequestParticipator : IParticipateInRequests<ProductByIdRequest, ProductByIdResponse>
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

        ongoingComposeResult.Response.Name = "My first product";
        ongoingComposeResult.Response.Description = "My first description";
    }
}