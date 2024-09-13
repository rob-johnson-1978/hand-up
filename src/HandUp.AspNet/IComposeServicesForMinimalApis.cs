using Microsoft.AspNetCore.Http;

namespace HandUp.AspNet;

public interface IComposeServicesForMinimalApis
{
    Task<IResult> ComposeEndpointAsync<TRequest, TResponse>(
        TRequest request,
        TResponse response,
        Func<TResponse, IResult>? onSuccess = null,
        Func<TResponse, IResult>? onNotFoundOrNoResults = null,
        Func<IReadOnlyCollection<string>, IResult>? onError = null);

    Task<IResult> ComposeEndpointAsync<TRequest, TResponse>(
        TRequest requestForAsync,
        TResponse responseForAsync,
        Func<TResponse, Task<IResult>>? onSuccessAsync = null,
        Func<TResponse, Task<IResult>>? onNotFoundOrNoResultsAsync = null,
        Func<IReadOnlyCollection<string>, Task<IResult>>? onErrorAsync = null);
}