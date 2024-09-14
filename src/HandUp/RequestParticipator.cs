namespace HandUp;

public abstract class RequestParticipator<TRequest, TResponse>
{
    public virtual bool IsStructureInitializer => false;

    public virtual bool Ready(TRequest request, ComposeResult<TResponse> ongoingComposeResult) => true;

    public virtual async Task RollbackAsync(TRequest request, ComposeResult<TResponse> ongoingComposeResult) => await Task.CompletedTask;

    public abstract Task ParticipateAsync(TRequest request, ComposeResult<TResponse> ongoingComposeResult);
}