using B2BShop.Api.Data;
using B2BShop.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace B2BShop.Api.Tests;

public class ProductTests
{
    [Fact]
    public async Task SeedProducts_CanSaveAdditionalProduct()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        await factory.InitializeDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<ShopDbContext>();

        await DbInitializer.SeedAsync(context);
        var countBefore = await context.Products.CountAsync();

        // Act
        var product = new Product("Orange Juice", 299);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(product.Id > 0);

        var products = await context.Products
            .AsNoTracking()
            .ToListAsync();

        Assert.Equal(countBefore + 1, products.Count);
        Assert.Contains(products, p =>
            p.Id == product.Id &&
            p.Name == "Orange Juice" &&
            p.PriceInCents == 299);
    }
}