using Contracts;
using HandUp;

namespace ProductDetailsService;

public class ProductsBySearchTermParticipator : IParticipateInRequests<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public bool WillPopulateCollectionSkeleton => true;

    public bool Ready(ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult) => true;

    public async Task ParticipateAsync(ProductsBySearchTermRequest request, ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (request.SearchTerm != "abc")
        {
            ongoingComposeResult.NotFoundOrNoResults = true;
            ongoingComposeResult.CollectionSkeletonReady = true;
            return;
        }

        ongoingComposeResult.Response.Add(new ProductBySearchTerm
        {
            Id = 1,
            Name = "Chocolate Hobnobs",
            Description = "Literally the best biscuits on the planet"
        });

        ongoingComposeResult.Response.Add(new ProductBySearchTerm
        {
            Id = 2,
            Name = "Twinings English Breakfast Tea Bags",
            Description = "Tea doesn't get any finer"
        });

        ongoingComposeResult.Response.Add(new ProductBySearchTerm
        {
            Id = 3,
            Name = "Kettle",
            Description = "A fancy one"
        });

        ongoingComposeResult.CollectionSkeletonReady = true;
    }
}