namespace HandUp;

public record ComposeResult<TResponse>(TResponse Response)
    where TResponse : class
{
    public bool NotFoundOrNoResults { get; set; }
    public bool CollectionSkeletonReady { get; set; }
    public List<string> Errors { get; set; } = [];
}