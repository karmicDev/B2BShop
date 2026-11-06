namespace B2BShop.Api.Contracts;

public record CartItemResponse(
    int ProductId,
    string Name,
    int Quantity,
    int UnitPriceInCents)
{
    public long TotalPriceInCents =>
        (long)Quantity * UnitPriceInCents;
}