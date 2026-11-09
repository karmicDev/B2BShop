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
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));

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
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));
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
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));
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
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));

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
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));
        
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
    
    [Fact]
    public async Task SetCartItem_ConcurrentRequestsForSameProduct_DoNotCreateDuplicateItems()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));

        using var createResponse =
            await client.PostAsync("/api/carts", null);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);

        var itemUrl = $"{location}/items/2";

        // Act
        var firstTask = client.PutAsJsonAsync(
            itemUrl, new SetCartItemRequest(2));

        var secondTask = client.PutAsJsonAsync(
            itemUrl, new SetCartItemRequest(2));

        await Task.WhenAll(firstTask, secondTask);

        using var firstResponse = await firstTask;
        using var secondResponse = await secondTask;

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

        using var getResponse = await client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var cart = await getResponse.Content
            .ReadFromJsonAsync<CartResponse>();

        Assert.NotNull(cart);

        var item = Assert.Single(cart.Items);

        Assert.Equal(2, item.ProductId);
        Assert.Equal("Apple Juice", item.Name);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(249, item.UnitPriceInCents);
        Assert.Equal(498L, item.TotalPriceInCents);
        Assert.Equal(498L, cart.TotalPriceInCents);
    }

    [Fact]
    public async Task IncrementCartItem_TenConcurrentRequestsProduceQuantityTen()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));

        using var createResponse =
            await client.PostAsync("/api/carts", null);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);

        var incrementUrl = $"{location}/items/2/increment";

        // Act
        var requests = Enumerable.Range(0, 10)
            .Select(async _ =>
            {
                using var response =
                    await client.PostAsync(incrementUrl, null);

                return response.StatusCode;
            })
            .ToArray();

        var statusCodes = await Task.WhenAll(requests);

        // Assert
        Assert.All(statusCodes, statusCode =>
            Assert.Equal(HttpStatusCode.NoContent, statusCode));

        using var getResponse = await client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var cart = await getResponse.Content
            .ReadFromJsonAsync<CartResponse>();

        Assert.NotNull(cart);

        var item = Assert.Single(cart.Items);

        Assert.Equal(2, item.ProductId);
        Assert.Equal(10, item.Quantity);
        Assert.Equal(249, item.UnitPriceInCents);
        Assert.Equal(2490L, item.TotalPriceInCents);
        Assert.Equal(2490L, cart.TotalPriceInCents);
    }
    
    [Theory]
    [InlineData(3, 2)]
    [InlineData(1, 0)]
    [InlineData(0, 0)]
    public async Task DecrementCartItem_ReducesQuantityOrRemovesItem(
        int initialQuantity, 
        int expectedQuantity) 
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));
    
        using var createResponse =
            await client.PostAsync("/api/carts", null);
    
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    
        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
    
        var itemUrl = $"{location}/items/2";
    
        if (initialQuantity > 0)
        {
            using var setResponse = await client.PutAsJsonAsync(
                itemUrl, new SetCartItemRequest(initialQuantity));
    
            Assert.Equal(HttpStatusCode.NoContent, setResponse.StatusCode);
        }
    
        // Act
        using var decrementResponse =
            await client.PostAsync($"{itemUrl}/decrement", null);
    
        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            decrementResponse.StatusCode);
    
        using var getResponse = await client.GetAsync(location);
    
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    
        var cart = await getResponse.Content
            .ReadFromJsonAsync<CartResponse>();
    
        Assert.NotNull(cart);
    
        if (expectedQuantity == 0)
        {
            Assert.Empty(cart.Items);
            Assert.Equal(0L, cart.TotalPriceInCents);
        }
        else
        {
            var item = Assert.Single(cart.Items);
    
            Assert.Equal(2, item.ProductId);
            Assert.Equal(expectedQuantity, item.Quantity);
            Assert.Equal(249, item.UnitPriceInCents);
            Assert.Equal(expectedQuantity * 249L, item.TotalPriceInCents);
            Assert.Equal(expectedQuantity * 249L, cart.TotalPriceInCents);
        }
    }
    
    [Fact]
    public async Task DecrementCartItem_TenConcurrentRequestsReduceQuantityFromElevenToOne()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        await factory.InitializeDatabaseAsync(new Product(2, "Apple Juice", 249));

        using var createResponse =
            await client.PostAsync("/api/carts", null);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);

        var itemUrl = $"{location}/items/2";

        using var setResponse = await client.PutAsJsonAsync(
            itemUrl, new SetCartItemRequest(11));

        Assert.Equal(HttpStatusCode.NoContent, setResponse.StatusCode);

        // Act
        var requests = Enumerable.Range(0, 10)
            .Select(async _ =>
            {
                using var response =
                    await client.PostAsync($"{itemUrl}/decrement", null);

                return response.StatusCode;
            })
            .ToArray();

        var statusCodes = await Task.WhenAll(requests);

        // Assert
        Assert.All(statusCodes, statusCode =>
            Assert.Equal(HttpStatusCode.NoContent, statusCode));

        using var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var cart = await getResponse.Content
            .ReadFromJsonAsync<CartResponse>();

        Assert.NotNull(cart);

        var item = Assert.Single(cart.Items);

        Assert.Equal(2, item.ProductId);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(249, item.UnitPriceInCents);
        Assert.Equal(249L, item.TotalPriceInCents);
        Assert.Equal(249L, cart.TotalPriceInCents);
    }
    
    [Fact]
    public async Task RemoveCartItem_RemovesEntireQuantityAndPreservesOtherItems()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
    
        await factory.InitializeDatabaseAsync(
            new Product(2, "Apple Juice", 249), 
            new Product(3, "Coffee Beans", 1299));
    
        using var createResponse =
            await client.PostAsync("/api/carts", null);
    
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    
        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
    
        var itemUrl = $"{location}/items/2";
    
        using var juiceResponse = await client.PutAsJsonAsync(
            itemUrl, new SetCartItemRequest(10));
    
        using var coffeeResponse = await client.PutAsJsonAsync(
            $"{location}/items/3", new SetCartItemRequest(2));
    
        Assert.Equal(HttpStatusCode.NoContent, juiceResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, coffeeResponse.StatusCode);
    
        // Act
        using var deleteResponse = await client.DeleteAsync(itemUrl);
    
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    
        using var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    
        var cart = await getResponse.Content
            .ReadFromJsonAsync<CartResponse>();
    
        Assert.NotNull(cart);
    
        var remainingItem = Assert.Single(cart.Items);
    
        Assert.Equal(3, remainingItem.ProductId);
        Assert.Equal(2, remainingItem.Quantity);
        Assert.Equal(2598L, remainingItem.TotalPriceInCents);
        Assert.Equal(2598L, cart.TotalPriceInCents);
    
        // Repeating DELETE must also succeed.
        using var repeatResponse = await client.DeleteAsync(itemUrl);
    
        Assert.Equal(HttpStatusCode.NoContent, repeatResponse.StatusCode);
    
        using var repeatGetResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, repeatGetResponse.StatusCode);
    
        var unchangedCart = await repeatGetResponse.Content
            .ReadFromJsonAsync<CartResponse>();
    
        Assert.NotNull(unchangedCart);
    
        var unchangedItem = Assert.Single(unchangedCart.Items);
    
        Assert.Equal(3, unchangedItem.ProductId);
        Assert.Equal(2, unchangedItem.Quantity);
        Assert.Equal(2598L, unchangedCart.TotalPriceInCents);
    }
}
