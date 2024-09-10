namespace HandUp;

public interface IParticipateInRequests<in TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    bool WillPopulateCollectionSkeleton { get; }
    bool Ready(ComposeResult<TResponse> ongoingComposeResult);
    Task ParticipateAsync(TRequest request, ComposeResult<TResponse> ongoingComposeResult);
}