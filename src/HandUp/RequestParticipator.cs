namespace HandUp;

public abstract class RequestParticipator<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    public virtual bool WillPopulateCollectionSkeleton { get; } = false;
    
    public virtual bool Ready(ComposeResult<TResponse> ongoingComposeResult) => true;

    public virtual async Task RollbackAsync(TRequest request, ComposeResult<TResponse> ongoingComposeResult) => await Task.CompletedTask;

    public abstract Task ParticipateAsync(TRequest request, ComposeResult<TResponse> ongoingComposeResult);
}