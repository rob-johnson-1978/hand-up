using Contracts;
using HandUp;

namespace ProductReviewService;

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

        ongoingComposeResult.Response.Reviews =
        [
            new ProductReview(5, "Really good!"),
            new ProductReview(4, "Packing was torn, but it didn't matter"),
            new ProductReview(1, "I hate everything")
        ];
    }
}