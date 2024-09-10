using Contracts;
using HandUp;
using Microsoft.Extensions.Logging;

namespace CustomerService;

public class CompleteCheckoutRequestParticipator(ILogger<CompleteCheckoutRequestParticipator> logger) : RequestParticipator<CompleteCheckoutRequest, CompleteCheckoutResponse>
{
    public override async Task ParticipateAsync(CompleteCheckoutRequest request, ComposeResult<CompleteCheckoutResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (request.CustomerId != Guid.Parse("b22aa987-c172-4b87-9168-58f89e697f8e"))
        {
            throw new InvalidOperationException($"Customer {request.CustomerId} does not exist");
        }

        // todo: ship
    }

    public override async Task RollbackAsync(CompleteCheckoutRequest request, ComposeResult<CompleteCheckoutResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        logger.LogInformation("!!!!!!! HERE !!!!!!!!!!!");
    }
}