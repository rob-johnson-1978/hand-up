namespace HandUp.ServiceComposition;

internal class ServiceComposer : IComposeServices
{
    public async Task<TResponse> ComposeAsync<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : class where TResponse : class
    {
        // todo:
        await Task.CompletedTask;
        return response;
    }
}