using B2BShop.Api.Data;
using B2BShop.Api.Models;
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