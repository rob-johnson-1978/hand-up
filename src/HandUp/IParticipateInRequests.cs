﻿namespace HandUp;

public interface IParticipateInRequests<in TRequest, in TResponse>
    where TRequest : class
    where TResponse : class
{
    bool WillPopulateCollectionSkeleton { get; }
    bool Ready(TResponse response);
    Task ParticipateAsync(TRequest request, TResponse response);
}