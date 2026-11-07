using B2BShop.Api.Data;
using B2BShop.Api.Models;
using B2BShop.Api.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
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
        var cart = await _dbContext.Carts
            .AsNoTracking()
            .Include(cart => cart.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(cart => cart.Id == id);

        if (cart is null)
        {
            return null;
        }

        var items = cart.Items
            .OrderBy(item => item.ProductId)
            .Select(item => new CartItemResponse(
                item.ProductId,
                item.Product.Name,
                item.Quantity,
                item.Product.PriceInCents))
            .ToList();

        return new CartResponse(cart.Id, items);
    }

    public async Task<SetCartItemResult> SetItemAsync(
        Guid cartId,
        int productId,
        int quantity)
    {
        if (quantity < 1 || quantity > 99)
        {
            return SetCartItemResult.InvalidQuantity;
        }

        if (!await _dbContext.Carts.AnyAsync(cart => cart.Id == cartId))
        {
            return SetCartItemResult.CartNotFound;
        }

        if (!await _dbContext.Products.AnyAsync(product => product.Id == productId))
        {
            return SetCartItemResult.ProductNotFound;
        }

        var item = await _dbContext.CartItems
            .FirstOrDefaultAsync(item =>
                item.CartId == cartId && item.ProductId == productId);

        if (item is not null)
        {
            item.Quantity = quantity;
            await _dbContext.SaveChangesAsync();

            return SetCartItemResult.Success;
        }

        var newItem = new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity
        };

        _dbContext.CartItems.Add(newItem);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException
                      { SqliteExtendedErrorCode: 2067 })
        {
            _dbContext.Entry(newItem).State = EntityState.Detached;

            var existingItem = await _dbContext.CartItems
                .SingleOrDefaultAsync(i =>
                    i.CartId == cartId && i.ProductId == productId);

            if (existingItem is null)
            {
                throw;
            }

            existingItem.Quantity = quantity;
            await _dbContext.SaveChangesAsync();
        }

        return SetCartItemResult.Success;
    }
}