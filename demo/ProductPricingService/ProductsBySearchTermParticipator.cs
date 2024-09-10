using Contracts;
using HandUp;

namespace ProductPricingService;

public class ProductsBySearchTermParticipator : IParticipateInRequests<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public bool WillPopulateCollectionSkeleton => false;

    public bool Ready(List<ProductBySearchTerm> response) => response.Count > 0;

    public async Task ParticipateAsync(ProductsBySearchTermRequest request, List<ProductBySearchTerm> response)
    {
        await Task.CompletedTask;

        // todo: this would be a db lookup
        foreach (var productBySearchTerm in response)
        {
            productBySearchTerm.CurrentPrice = productBySearchTerm.Id switch
            {
                1 => 2.99M,
                2 => 1.49M,
                3 => 49.99M,
                _ => throw new NotImplementedException()
            };
        }
    }
}