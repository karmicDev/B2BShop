using B2BShop.Api.Data;
using B2BShop.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace B2BShop.Api.Tests;

public class ShopApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"B2BShop-tests-{Guid.NewGuid():N}.db");

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
                options.UseSqlite(
                    $"Data Source={_databasePath};Pooling=False"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            File.Delete(_databasePath);
            File.Delete($"{_databasePath}-wal");
            File.Delete($"{_databasePath}-shm");
            File.Delete($"{_databasePath}-journal");
        }
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
}