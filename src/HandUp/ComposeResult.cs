namespace HandUp;

public record ComposeResult<TResponse>(TResponse Response)
{
    public bool NotFoundOrNoResults { get; set; }
    public bool StructureInitialized { get; set; }
    public List<string> Errors { get; } = [];
    public bool HasErrors => Errors.Count > 0;
    public dynamic TempData { get; } = new { };
}