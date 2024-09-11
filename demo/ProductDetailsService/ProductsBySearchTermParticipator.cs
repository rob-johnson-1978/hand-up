using Contracts;
using HandUp;

namespace ProductDetailsService;

public class ProductsBySearchTermParticipator : RequestParticipator<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public override bool IsStructureInitializer => true;

    public override async Task ParticipateAsync(ProductsBySearchTermRequest request, ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (request.SearchTerm != "abc")
        {
            ongoingComposeResult.NotFoundOrNoResults = true;
            ongoingComposeResult.StructureInitialized = true;
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

        ongoingComposeResult.StructureInitialized = true;
    }
}