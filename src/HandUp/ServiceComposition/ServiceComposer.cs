using Microsoft.Extensions.DependencyInjection;

namespace HandUp.ServiceComposition;

internal class ServiceComposer(HandUpConfiguration options, IServiceProvider scopedServiceProvider) : IComposeServices
{
    private int count;

    public async Task ComposeAsync<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : class where TResponse : class
    {
        var participators = scopedServiceProvider
            .GetServices<IParticipateInRequests<TRequest, TResponse>>()
            .OrderByDescending(x => x.WillPopulateCollectionSkeleton)
            .ToList();

        if (participators.Count < 1)
        {
            throw new InvalidOperationException($"No participators are available for request {request} / response {response}. There must be at least one.");
        }

        var numberOfCollectionSkeletonPopulators = participators.Count(x => x.WillPopulateCollectionSkeleton);

        if (numberOfCollectionSkeletonPopulators > 1)
        {
            throw new InvalidOperationException($"More than one participator ({numberOfCollectionSkeletonPopulators}) has "
                                                + $"'{nameof(IParticipateInRequests<object, object>.WillPopulateCollectionSkeleton)}' set to true "
                                                + $"for request {request} / response {response}. There can only be one per composition.");
        }

        var hasCollectionSkeletonPopulator = numberOfCollectionSkeletonPopulators == 1;

        await SetResponseAsync(participators, request, response, hasCollectionSkeletonPopulator).ConfigureAwait(false);
    }

    private async Task SetResponseAsync<TRequest, TResponse>(
        List<IParticipateInRequests<TRequest, TResponse>> participators, 
        TRequest request, 
        TResponse response, 
        bool hasCollectionSkeletonPopulator)
        where TRequest : class
        where TResponse : class
    {
        if (participators.Count < 1)
        {
            return; // all complete
        }

        if (count > options.MaxParticipationLoopCount)
        {
            throw new InvalidOperationException($"Participating loop has reached maximum attempts for request {request} / response {response}. Do you have a participator which never becomes ready?");
        }

        if (hasCollectionSkeletonPopulator)
        {
            var firstParticipator = participators.First();
            await firstParticipator.ParticipateAsync(request, response).ConfigureAwait(false);
            participators.Remove(firstParticipator);
        }

        var readyParticipators = participators.Where(x => x.Ready(response)).ToArray();

        if (readyParticipators.Length < 1)
        {
            throw new InvalidOperationException($"No participators are ready for request {request} / response {response} despite there being at least one still waiting to participate.");
        }

        await Task.WhenAll(readyParticipators.Select(p => p.ParticipateAsync(request, response))).ConfigureAwait(false);

        foreach (var readyParticipator in readyParticipators)
        {
            participators.Remove(readyParticipator);
        }

        count++;

        await SetResponseAsync(participators, request, response, hasCollectionSkeletonPopulator: false).ConfigureAwait(false);
    }
}