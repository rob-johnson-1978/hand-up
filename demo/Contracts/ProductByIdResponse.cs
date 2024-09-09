namespace Contracts;

public record ProductByIdResponse
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public IReadOnlyCollection<ProductReview> Reviews { get; set; } = [];
}