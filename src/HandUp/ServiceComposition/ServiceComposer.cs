using Microsoft.Extensions.DependencyInjection;

namespace HandUp.ServiceComposition;

internal class ServiceComposer(IServiceProvider scopedServiceProvider) : IComposeServices
{
    public async Task<TResponse> ComposeAsync<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : class where TResponse : class
    {
        var participators = scopedServiceProvider.GetServices<IParticipateInRequests<TRequest, TResponse>>();
        
        // todo:
        await Task.CompletedTask;
        return response;
    }
}