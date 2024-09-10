namespace HandUp;

public interface IComposeServices
{
    Task<ComposeResult<TResponse>> ComposeAsync<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : class
        where TResponse : class;
}