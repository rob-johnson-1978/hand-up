namespace Contracts;

public record ProductBySearchTerm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public int AverageReview { get; set; }
    public int StockLevel { get; set; }
}