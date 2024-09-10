using Contracts;
using HandUp;

namespace ProductDetailsService;

public class ProductsBySearchTermParticipator : IParticipateInRequests<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public bool WillPopulateCollectionSkeleton => true;

    public bool Ready(List<ProductBySearchTerm> response) => true;

    public async Task ParticipateAsync(ProductsBySearchTermRequest request, List<ProductBySearchTerm> response)
    {
        await Task.CompletedTask;

        response.Add(new ProductBySearchTerm
        {
            Id = 1,
            Name = "Chocolate Hobnobs",
            Description = "Literally the best biscuits on the planet"
        });

        response.Add(new ProductBySearchTerm
        {
            Id = 2,
            Name = "Twinings English Breakfast Tea Bags",
            Description = "Tea doesn't get any finer"
        });

        response.Add(new ProductBySearchTerm
        {
            Id = 3,
            Name = "Kettle",
            Description = "A fancy one"
        });
    }
}