namespace B2BShop.Api.Models;

public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public List<CartItem> Items { get; set; } = [];
    
    public required string UserId { get; init; }
    
    public ApplicationUser? User { get; set; }
}