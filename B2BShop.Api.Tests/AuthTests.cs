using System.Net;
using System.Net.Http.Json;

namespace B2BShop.Api.Tests;

public class AuthTests
{
    [Fact]
    public async Task GetCurrentUser_WithoutLoginReturnsUnauthorized()
    {
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLogin_CanRetrieveCurrentUser()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        using var client = factory.CreateClient();

        await factory.InitializeDatabaseAsync();

        var credentials = new
        {
            email = "learner@example.com",
            password = "Learning-Shop-42!"
        };

        // Act
        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register", credentials);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login?useCookies=true", credentials);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var userResponse = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);

        var user = await userResponse.Content
            .ReadFromJsonAsync<CurrentUserResponse>();

        Assert.NotNull(user);
        Assert.False(string.IsNullOrWhiteSpace(user.Id));
        Assert.Equal(credentials.email, user.Email);
    }

    public record CurrentUserResponse(string Id, string Email);
}