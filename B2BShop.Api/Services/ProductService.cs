using B2BShop.Api.Data;
using B2BShop.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace B2BShop.Api.Services;

public class ProductService
{
    private readonly ShopDbContext _dbContext;

    public ProductService(ShopDbContext context)
    {
        _dbContext = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _dbContext.Products
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);
    }
}