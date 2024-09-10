using Contracts;
using HandUp;

namespace ProductReviewService;

public class ProductByIdRequestParticipator : IParticipateInRequests<ProductByIdRequest, ProductByIdResponse>
{
    public bool WillPopulateCollectionSkeleton => false;

    public bool Ready(ProductByIdResponse response) => true; // always ready

    public async Task ParticipateAsync(ProductByIdRequest request, ProductByIdResponse response)
    {
        await Task.CompletedTask;

        response.Reviews =
        [
            new ProductReview(5, "Really good!"),
            new ProductReview(4, "Packing was torn, but it didn't matter"),
            new ProductReview(1, "I hate everything")
        ];
    }
}