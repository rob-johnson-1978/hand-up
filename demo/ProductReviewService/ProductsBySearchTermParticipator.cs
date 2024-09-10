using Contracts;
using HandUp;

namespace ProductReviewService;

public class ProductsBySearchTermParticipator : IParticipateInRequests<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public bool WillPopulateCollectionSkeleton => false;

    public bool Ready(ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult) => ongoingComposeResult.Response.Count > 0;

    public async Task ParticipateAsync(ProductsBySearchTermRequest request, ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (ongoingComposeResult.NotFoundOrNoResults)
        {
            return;
        }

        // todo: this would be a db lookup
        foreach (var productBySearchTerm in ongoingComposeResult.Response)
        {
            productBySearchTerm.AverageReview = productBySearchTerm.Id switch
            {
                1 => 4,
                2 => 5,
                3 => 3,
                _ => throw new NotImplementedException()
            };
        }
    }
}