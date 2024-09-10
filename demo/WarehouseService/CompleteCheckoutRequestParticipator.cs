using Contracts;
using HandUp;

namespace WarehouseService;

public class CompleteCheckoutRequestParticipator : RequestParticipator<CompleteCheckoutRequest, CompleteCheckoutResponse>
{
    public override async Task ParticipateAsync(CompleteCheckoutRequest request, ComposeResult<CompleteCheckoutResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        // todo: ship
    }
}