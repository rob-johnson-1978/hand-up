using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HandUp.ServiceComposition;

internal class ServiceComposer(
    ILogger<ServiceComposer> logger,
    HandUpConfiguration options,
    IServiceProvider scopedServiceProvider) : IComposeServices
{
    private int count;

    public async Task<ComposeResult<TResponse>> ComposeAsync<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : class where TResponse : class
    {
        var allParticipators = scopedServiceProvider
            .GetServices<RequestParticipator<TRequest, TResponse>>()
            .OrderByDescending(x => x.WillPopulateCollectionSkeleton)
            .ToArray();

        if (allParticipators.Length < 1)
        {
            throw new InvalidOperationException($"No participators are available for request {request} / response {response}. There must be at least one.");
        }

        var numberOfCollectionSkeletonPopulators = allParticipators.Count(x => x.WillPopulateCollectionSkeleton);

        if (numberOfCollectionSkeletonPopulators > 1)
        {
            throw new InvalidOperationException($"More than one participator ({numberOfCollectionSkeletonPopulators}) has "
                                                + $"'{nameof(RequestParticipator<object, object>.WillPopulateCollectionSkeleton)}' set to true "
                                                + $"for request {request} / response {response}. There can only be one per composition.");
        }

        var hasCollectionSkeletonPopulator = numberOfCollectionSkeletonPopulators == 1;

        var ongoingComposeResult = new ComposeResult<TResponse>(response);

        var currentParticipators = new List<RequestParticipator<TRequest, TResponse>>(allParticipators.Length);
        currentParticipators.AddRange(allParticipators);

        await SetResponseAsync(allParticipators, currentParticipators, request, ongoingComposeResult, hasCollectionSkeletonPopulator).ConfigureAwait(false);

        return ongoingComposeResult;
    }

    private async Task SetResponseAsync<TRequest, TResponse>(
        RequestParticipator<TRequest, TResponse>[] allParticipators,
        List<RequestParticipator<TRequest, TResponse>> currentParticipators,
        TRequest request,
        ComposeResult<TResponse> ongoingComposeResult,
        bool hasCollectionSkeletonPopulator)
        where TRequest : class
        where TResponse : class
    {
        if (currentParticipators.Count < 1)
        {
            return; // all complete
        }

        if (ongoingComposeResult.NotFoundOrNoResults || ongoingComposeResult.Errors.Count > 0)
        {
            return;
        }

        if (count > options.MaxParticipationLoopCount)
        {
            throw new InvalidOperationException($"Participating loop has reached maximum attempts for request {request} / response {ongoingComposeResult}. Do you have a participator which never becomes ready?");
        }

        try
        {
            if (hasCollectionSkeletonPopulator)
            {
                var firstParticipator = currentParticipators.First();
                await firstParticipator.ParticipateAsync(request, ongoingComposeResult).ConfigureAwait(false);
                currentParticipators.Remove(firstParticipator);
            }
        }
        catch (Exception ex)
        {
            HandleException<TRequest, TResponse>(ex, ongoingComposeResult.Errors);
            await RollbackAsync(allParticipators, request, ongoingComposeResult).ConfigureAwait(false);
            return;
        }

        var readyParticipators = currentParticipators.Where(x => x.Ready(ongoingComposeResult)).ToArray();

        if (readyParticipators.Length < 1)
        {
            throw new InvalidOperationException($"No participators are ready for request {request} / response {ongoingComposeResult} despite there being at least one still waiting to participate.");
        }

        try
        {
            await Task.WhenAll(readyParticipators.Select(p => p.ParticipateAsync(request, ongoingComposeResult))).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException<TRequest, TResponse>(ex, ongoingComposeResult.Errors);
            await RollbackAsync(allParticipators, request, ongoingComposeResult).ConfigureAwait(false);
            return;
        }

        foreach (var readyParticipator in readyParticipators)
        {
            currentParticipators.Remove(readyParticipator);
        }

        count++;

        await SetResponseAsync(allParticipators, currentParticipators, request, ongoingComposeResult, hasCollectionSkeletonPopulator: false).ConfigureAwait(false);
    }

    private void HandleException<TRequest, TResponse>(Exception ex, List<string> errors)
        where TRequest : class
        where TResponse : class
    {
        const string publicError = "An exception was handled during service participation. See logs";
        errors.Add(publicError);

        logger.LogError(ex, "An exception was handled during request handling for request {requestType} and response {responseType}. Attempting rollback", typeof(TRequest), typeof(TResponse));
    }

    private async Task RollbackAsync<TRequest, TResponse>(RequestParticipator<TRequest, TResponse>[] allParticipators, TRequest request, ComposeResult<TResponse> response)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            await Task.WhenAll(allParticipators.Select(x => x.RollbackAsync(request, response)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occured during rollback for request {requestType} and response {responseType}.", typeof(TRequest), typeof(TResponse));
        }
    }
}