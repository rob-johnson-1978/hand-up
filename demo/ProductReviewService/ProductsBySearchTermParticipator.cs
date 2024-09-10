using Contracts;
using HandUp;

namespace ProductReviewService;

public class ProductsBySearchTermParticipator : RequestParticipator<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public override bool Ready(ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult) => ongoingComposeResult.CollectionSkeletonReady;

    public override async Task ParticipateAsync(ProductsBySearchTermRequest request, ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult)
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