using System.Net;
using System.Net.Http.Json;
using B2BShop.Api.Contracts;
using B2BShop.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace B2BShop.Api.Tests;

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
}
