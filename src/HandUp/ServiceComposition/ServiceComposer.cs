using Microsoft.Extensions.DependencyInjection;

namespace HandUp.ServiceComposition;

internal class ServiceComposer(HandUpConfiguration options, IServiceProvider scopedServiceProvider) : IComposeServices
{
    private int count;
    
    public async Task ComposeAsync<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : class where TResponse : class
    {
        var participators = scopedServiceProvider.GetServices<IParticipateInRequests<TRequest, TResponse>>().ToArray();

        if (participators.Length < 1)
        {
            throw new InvalidOperationException($"No participators are available for request {request} / response {response}. There must be at least one.");
        }
            
        await BuildResponseAsync(participators, request, response).ConfigureAwait(false);
    }

    private async Task BuildResponseAsync<TRequest, TResponse>(IParticipateInRequests<TRequest, TResponse>[] participators, TRequest request, TResponse response)
        where TRequest : class where TResponse : class
    {
        if (count > options.MaxParticipationLoopCount)
        {
            throw new InvalidOperationException($"Participating loop has reached maximum attempts for request {request} / response {response}. Do you have a participator which never becomes ready?");
        }

        var readyParticipators = participators.Where(x => x.Ready(response)).ToArray();

        if (readyParticipators.Length < 1)
        {
            throw new InvalidOperationException($"No participators are ready for request {request} / response {response} despite there being at least one still waiting to participate.");
        }

        var finalPass = participators.Length == readyParticipators.Length;

        await Task.WhenAll(readyParticipators.Select(p => p.ParticipateAsync(request, response))).ConfigureAwait(false);

        if (finalPass)
        {
            return;
        }

        count++;

        await BuildResponseAsync(participators, request, response).ConfigureAwait(false);
    }
}