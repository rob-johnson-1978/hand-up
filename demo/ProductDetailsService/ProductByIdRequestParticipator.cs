using Contracts;
using HandUp;

namespace ProductDetailsService;

public class ProductByIdRequestParticipator : IParticipateInRequests<ProductByIdRequest, ProductByIdResponse>
{
    public bool WillPopulateCollectionSkeleton => false;

    public bool Ready(ProductByIdResponse response) => true; // always ready

    public async Task ParticipateAsync(ProductByIdRequest request, ProductByIdResponse response)
    {
        await Task.CompletedTask;

        response.Name = "My first product";
        response.Description = "My first description";
    }
}