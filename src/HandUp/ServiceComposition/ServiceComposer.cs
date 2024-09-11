using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HandUp.ServiceComposition;

internal class ServiceComposer(
    ILogger<ServiceComposer> logger,
    HandUpConfiguration configuration,
    IServiceProvider scopedServiceProvider) : IComposeServices
{
    private int count;

    public async Task<ComposeResult<TResponse>> ComposeAsync<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : class where TResponse : class
    {
        var remainingParticipators = scopedServiceProvider
            .GetServices<RequestParticipator<TRequest, TResponse>>()
            .OrderByDescending(x => x.IsStructureInitializer)
            .ToList();

        if (remainingParticipators.Count < 1)
        {
            throw new InvalidOperationException($"No participators are available for request {request} / response {response}. There must be at least one.");
        }

        var numberOfStructureInitializers = remainingParticipators.Count(x => x.IsStructureInitializer);

        if (numberOfStructureInitializers > 1)
        {
            throw new InvalidOperationException($"More than one participator ({numberOfStructureInitializers}) has "
                                                + $"'{nameof(RequestParticipator<object, object>.IsStructureInitializer)}' set to true "
                                                + $"for request {request} / response {response}. There can only be one per composition.");
        }

        var hasStructureInitializer = numberOfStructureInitializers == 1;

        var ongoingComposeResult = new ComposeResult<TResponse>(response);

        await BuildResultAsync(remainingParticipators, completedParticipators: [], request, ongoingComposeResult, hasStructureInitializer).ConfigureAwait(false);

        return ongoingComposeResult;
    }

    private async Task BuildResultAsync<TRequest, TResponse>(
        List<RequestParticipator<TRequest, TResponse>> remainingParticipators,
        List<RequestParticipator<TRequest, TResponse>> completedParticipators,
        TRequest request,
        ComposeResult<TResponse> ongoingComposeResult,
        bool hasStructureInitializer)
        where TRequest : class
        where TResponse : class
    {
        if (remainingParticipators.Count < 1)
        {
            return; // all complete
        }

        if (ongoingComposeResult.NotFoundOrNoResults || ongoingComposeResult.HasErrors)
        {
            return;
        }

        if (count > configuration.MaxParticipationLoopCount)
        {
            throw new InvalidOperationException($"Participating loop has reached maximum attempts for request {request} / response {ongoingComposeResult}. Do you have a participator which never becomes ready?");
        }

        if (hasStructureInitializer)
        {
            var structureInitializerParticipator = remainingParticipators.First();

            try
            {
                await structureInitializerParticipator.ParticipateAsync(request, ongoingComposeResult).ConfigureAwait(false);

                remainingParticipators.Remove(structureInitializerParticipator);
                completedParticipators.Add(structureInitializerParticipator);

                if (remainingParticipators.Count < 1)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                HandleException<TRequest, TResponse>(ex, ongoingComposeResult.Errors);
                await RollbackAsync([structureInitializerParticipator], request, ongoingComposeResult).ConfigureAwait(false);
                return;
            }
        }

        var readyParticipators = remainingParticipators.Where(x => x.Ready(ongoingComposeResult)).ToArray();

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
            await RollbackAsync(readyParticipators, request, ongoingComposeResult).ConfigureAwait(false);
            return;
        }

        foreach (var readyParticipator in readyParticipators)
        {
            remainingParticipators.Remove(readyParticipator);
            completedParticipators.Add(readyParticipator);
        }

        count++;

        await BuildResultAsync(remainingParticipators, completedParticipators, request, ongoingComposeResult, hasStructureInitializer: false).ConfigureAwait(false);
    }

    private void HandleException<TRequest, TResponse>(Exception ex, List<string> errors)
        where TRequest : class
        where TResponse : class
    {
        const string publicError = "An exception was handled during service participation. See logs";
        errors.Add(publicError);

        logger.LogError(ex, "An exception was handled during request handling for request {requestType} and response {responseType}. Attempting rollback", typeof(TRequest), typeof(TResponse));
    }

    private async Task RollbackAsync<TRequest, TResponse>(RequestParticipator<TRequest, TResponse>[] participators, TRequest request, ComposeResult<TResponse> response)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            await Task.WhenAll(participators.Select(x => x.RollbackAsync(request, response)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occured during rollback for request {requestType} and response {responseType}.", typeof(TRequest), typeof(TResponse));
        }
    }
}