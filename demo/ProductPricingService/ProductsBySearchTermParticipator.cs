using Contracts;
using HandUp;

namespace ProductPricingService;

public class ProductsBySearchTermParticipator : RequestParticipator<ProductsBySearchTermRequest, List<ProductBySearchTerm>>
{
    public override bool Ready(ComposeResult<List<ProductBySearchTerm>> ongoingComposeResult) => ongoingComposeResult.StructureInitialized;

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