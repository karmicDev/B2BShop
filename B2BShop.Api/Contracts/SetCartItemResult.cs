namespace B2BShop.Api.Contracts;

public enum SetCartItemResult
{
    Success,
    InvalidQuantity,
    CartNotFound,
    ProductNotFound,
    Conflict
}