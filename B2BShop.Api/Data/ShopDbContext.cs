using B2BShop.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BShop.Api.Data;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}