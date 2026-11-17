using B2BShop.Api.Data;
using B2BShop.Api.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace B2BShop.Api.Tests;

public class ShopApiFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _database =
        new PostgreSqlBuilder("postgres:17")
            .Build();

    public Task StartDatabaseAsync()
    {
        return _database.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ShopDbContext>();
            services.RemoveAll<DbContextOptions<ShopDbContext>>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<ShopDbContext>>();

            services.AddDbContext<ShopDbContext>(options =>
                options.UseNpgsql(_database.GetConnectionString()));
        });
    }

    public async Task InitializeDatabaseAsync(params Product[] products)
    {
        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ShopDbContext>();

        await context.Database.MigrateAsync();

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Registers a user and returns a client that is logged in with a cookie.
    /// Call <see cref="InitializeDatabaseAsync"/> first so the schema exists.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        string email = "buyer@example.com",
        string password = "Learning-Shop-42!")
    {
        var client = CreateClient();
        var credentials = new { email, password };

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register", credentials);
        registerResponse.EnsureSuccessStatusCode();

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login?useCookies=true", credentials);
        loginResponse.EnsureSuccessStatusCode();

        return client;
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
        }
        finally
        {
            await _database.DisposeAsync();
        }
    }
}