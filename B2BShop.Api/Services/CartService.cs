using B2BShop.Api.Data;
using B2BShop.Api.Models;
using B2BShop.Api.Contracts;
using Microsoft.EntityFrameworkCore;
namespace B2BShop.Api.Services;

public class CartService
{
    private readonly ShopDbContext _dbContext;

    public CartService(ShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateAsync()
    {
        var cart = new Cart();
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();
        return cart.Id;
    }

    public async Task<CartResponse?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Carts
            .AsNoTracking()
            .Where(cart => cart.Id == id)
            .Select(cart => new CartResponse(cart.Id))
            .FirstOrDefaultAsync();
    }
}