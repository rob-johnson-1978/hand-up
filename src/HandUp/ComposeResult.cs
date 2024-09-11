namespace HandUp;

public record ComposeResult<TResponse>(TResponse Response)
    where TResponse : class
{
    public bool NotFoundOrNoResults { get; set; }
    public bool StructureInitialized { get; set; }
    public List<string> Errors { get; set; } = [];
    public bool HasErrors => Errors.Count > 0;
}