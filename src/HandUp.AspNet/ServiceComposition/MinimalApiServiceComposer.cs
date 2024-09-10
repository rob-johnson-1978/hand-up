using Microsoft.AspNetCore.Http;

namespace HandUp.AspNet.ServiceComposition;

internal class MinimalApiServiceComposer(IComposeServices serviceComposer) : IComposeServicesForMinimalApis
{
    public async Task<IResult> ComposeEndpointAsync<TRequest, TResponse>(
        TRequest request,
        TResponse response,
        Func<TResponse, IResult>? onSuccess = null,
        Func<TResponse, IResult>? onNotFoundOrNoResults = null,
        Func<IReadOnlyCollection<string>, IResult>? onError = null)
        where TRequest : class where TResponse : class
    {
        var result = await serviceComposer.ComposeAsync(request, response);

        if (result.HasErrors)
        {
            return onError == null
                       ? Results.Problem(string.Join(" / ", result.Errors))
                       : onError.Invoke(result.Errors);
        }

        if (result.NotFoundOrNoResults)
        {
            return onNotFoundOrNoResults == null
                       ? Results.NotFound()
                       : onNotFoundOrNoResults.Invoke(result.Response);
        }

        return onSuccess == null
                   ? Results.Ok(result.Response)
                   : onSuccess.Invoke(result.Response);
    }

    public async Task<IResult> ComposeEndpointAsync<TRequest, TResponse>(
        TRequest requestForAsync,
        TResponse responseForAsync,
        Func<TResponse, Task<IResult>>? onSuccessAsync = null,
        Func<TResponse, Task<IResult>>? onNotFoundOrNoResultsAsync = null,
        Func<IReadOnlyCollection<string>, Task<IResult>>? onErrorAsync = null)
        where TRequest : class where TResponse : class
    {
        var result = await serviceComposer.ComposeAsync(requestForAsync, responseForAsync);

        if (result.HasErrors)
        {
            return onErrorAsync == null
                       ? Results.Problem(string.Join(" / ", result.Errors))
                       : await onErrorAsync.Invoke(result.Errors);
        }

        if (result.NotFoundOrNoResults)
        {
            return onNotFoundOrNoResultsAsync == null
                       ? Results.NotFound()
                       : await onNotFoundOrNoResultsAsync.Invoke(result.Response);
        }

        return onSuccessAsync == null
                   ? Results.Ok(result.Response)
                   : await onSuccessAsync.Invoke(result.Response);
    }
}