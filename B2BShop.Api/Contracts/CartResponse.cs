using B2BShop.Api.Models;
using Microsoft.AspNetCore.Http.Features;

namespace B2BShop.Api.Contracts;

public record CartResponse(
    Guid Id,
    IReadOnlyList<CartItemResponse> Items)

{
    public long TotalPriceInCents => Items.Sum(item => item.TotalPriceInCents);
}