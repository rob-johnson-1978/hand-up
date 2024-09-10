namespace Contracts;

public record CompleteCheckoutRequest(Guid OrderId, Guid CustomerId, Guid AddressId, (int ProductId, int PurchasePrice)[] Items);