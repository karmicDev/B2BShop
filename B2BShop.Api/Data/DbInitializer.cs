using B2BShop.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace B2BShop.Api.Data;

public class DbInitializer
{
    public static async Task SeedAsync(ShopDbContext dbContext)
    {
        if (await dbContext.Products.AnyAsync())
        {
            return;
        }

        dbContext.Products.AddRange(
            new Product(1, "Mineral Water", 99),
            new Product(2, "Apple Juice", 249),
            new Product(3, "Coffee Beans", 1299)
        );
        await dbContext.SaveChangesAsync();
    }
}