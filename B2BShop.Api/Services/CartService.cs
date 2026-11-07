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

    public Task<SetCartItemResult> SetItemAsync(
        Guid cartId,
        int productId,
        int quantity)
    {
        if (quantity < 1 || quantity > 99)
        {
            return Task.FromResult(SetCartItemResult.InvalidQuantity);
        }

        return ChangeQuantityAsync(cartId, productId, _ => quantity);
    }

    public Task<SetCartItemResult> IncrementItemAsync(
        Guid cartId,
        int productId)
    {
        return ChangeQuantityAsync(
            cartId, productId, currentQuantity => currentQuantity + 1);
    }

    public Task<SetCartItemResult> DecrementItemAsync(
        Guid cartId,
        int productId)
    {
        return ChangeQuantityAsync(
            cartId, 
            productId, 
            currentQuantity => currentQuantity - 1,
            removeWhenEmpty: true);
    }

    public Task<SetCartItemResult> RemoveItemAsync(
        Guid cartId,
        int productId)
    {
        return ChangeQuantityAsync(
            cartId,
            productId,
            _ => 0,
            removeWhenEmpty: true);
    }

    private async Task<SetCartItemResult> ChangeQuantityAsync(
        Guid  cartId,
        int productId,
        Func<int, int> calculateQuantity,
        bool removeWhenEmpty = false)
    {
        if (!await _dbContext.Carts.AnyAsync(cart => cart.Id == cartId))
        {
            return SetCartItemResult.CartNotFound;
        }

        if (!await _dbContext.Products.AnyAsync(product => product.Id == productId))
        {
            return SetCartItemResult.ProductNotFound;
        }
        
        const int maxAttempts = 20;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var item = await _dbContext.CartItems.SingleOrDefaultAsync(
                item => item.CartId == cartId && item.ProductId == productId);
            var isNew = item is null;
            
            if (item is null && removeWhenEmpty)
            {
                return SetCartItemResult.Success;
            }
            
            var quantity = calculateQuantity(item?.Quantity ?? 0);
            
            if (removeWhenEmpty && quantity == 0)
            {
                _dbContext.CartItems.Remove(item!);
            }
            else
            {
                if (quantity < 1 || quantity > 99)
                {
                    return SetCartItemResult.InvalidQuantity;
                }

                if (item is null)
                {
                    item = new CartItem
                    {
                        CartId = cartId,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    _dbContext.CartItems.Add(item);
                }
                else
                {
                    item.Quantity = quantity;
                }

            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return SetCartItemResult.Success;
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.Entry(item!).State = EntityState.Detached;
            }
            catch (DbUpdateException exception)
                when (isNew &&
                      exception.InnerException is SqliteException
                          { SqliteExtendedErrorCode: 2067 })
            {
                _dbContext.Entry(item!).State = EntityState.Detached;
            }
            if (attempt < maxAttempts - 1)
            {
                await Task.Delay(Random.Shared.Next(5, 25));
            }
        }
        return SetCartItemResult.Conflict;
    }
}