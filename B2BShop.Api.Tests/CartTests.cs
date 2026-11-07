using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices.JavaScript;
using B2BShop.Api.Contracts;
using B2BShop.Api.Data;
using B2BShop.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace B2BShop.Api.Tests;

public record ApiError(string Error);

public class CartTests
{
    [Fact]
    public async Task CreateCart_CanBeRetrievedAsEmptyCart()
    {
        // Arrange
        using var factory = new ShopApiFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await context.Database.MigrateAsync();
        }

        // Act
        using var createResponse =  await client.PostAsync("api/carts", null);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);

        using var response = await client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
        Assert.NotNull(cart);
        Assert.NotEqual(Guid.Empty, cart.Id);
        Assert.Empty(cart.Items);
        Assert.Equal(0L, cart.TotalPriceInCents);
    }

    [Fact]
    public async Task SetCartItem_RepeatedRequestKeepsQuantityAndTotal()
    {
        // Arrange
        using var factory = new ShopApiFactory();
        using var client = factory.CreateClient();
        
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await context.Database.MigrateAsync();
            context.Products.Add(new Product(2, "Apple Juice", 249));
            await context.SaveChangesAsync();
        }
        
        using var createResponse = await client.PostAsync("api/carts", null);
        
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var location = createResponse.Headers.Location;
        var itemUrl =$"{location}/items/2";
        
        // Act
        using var firstResponse = await client.PutAsJsonAsync(
            itemUrl, new SetCartItemRequest(2));
        using var secondResponse = await client.PutAsJsonAsync(
            itemUrl, new SetCartItemRequest(2));
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);
        
        using var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var cart = await getResponse.Content.ReadFromJsonAsync<CartResponse>();
        Assert.NotNull(cart);
        
        var item = Assert.Single(cart.Items);
        
        Assert.Equal(2, item.ProductId);
        Assert.Equal("Apple Juice", item.Name);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(249, item.UnitPriceInCents);
        Assert.Equal(498L, item.TotalPriceInCents);
        Assert.Equal(498L, cart.TotalPriceInCents);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(100)]
    public async Task SetCartItem_InvalidQuantityReturnsBadRequest(int quantity)
    {
        // Arrange
        using var factory = new ShopApiFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await context.Database.MigrateAsync();
            context.Products.Add(new Product(2, "Apple Juice", 249));
            await context.SaveChangesAsync();
        }
        using var createResponse = await client.PostAsync("api/carts", null);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
        
        // Act
        using var response = await client.PutAsJsonAsync(
            $"{location}/items/2",
            new SetCartItemRequest(quantity));
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        using var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var cart = await getResponse.Content.ReadFromJsonAsync<CartResponse>();
        
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
        Assert.Equal(0L, cart.TotalPriceInCents);
    }

    [Fact]
    public async Task SetCartItem_UnknownCartReturnsNotFound()
    {
        // Arrange
        using var factory = new ShopApiFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<ShopDbContext>();

            await context.Database.MigrateAsync();

            context.Products.Add(new Product(2, "Apple Juice", 249));
            await context.SaveChangesAsync();
        }

        // Act
        var unknownCartId = Guid.NewGuid();

        using var response = await client.PutAsJsonAsync(
            $"/api/carts/{unknownCartId}/items/2",
            new SetCartItemRequest(2));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>();

        Assert.NotNull(error);
        Assert.Equal("Cart not found.", error.Error);
    }

    [Fact]
    public async Task SetCartItem_UnknownProductReturnsNotFound()
    {
        // Arrange
        using var factory = new ShopApiFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            await context.Database.MigrateAsync();
            context.Products.Add(new Product(2, "Apple Juice", 249));
            await context.SaveChangesAsync();
        }
        
        // Act
        using var createResponse = await client.PostAsync("api/carts", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var location = createResponse.Headers.Location;
        Assert.NotNull(location);

        using var response = await client.PutAsJsonAsync($"{location}/items/999",
            new SetCartItemRequest(2));
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal("Product not found.", error.Error);
        
        using var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var cart = await getResponse.Content.ReadFromJsonAsync<CartResponse>();
        
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
        Assert.Equal(0L, cart.TotalPriceInCents);
    }
}
