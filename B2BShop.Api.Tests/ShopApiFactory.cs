using System.Data.Common;
using B2BShop.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace B2BShop.Api.Tests;

public class ShopApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ShopDbContext>();
            services.RemoveAll<DbContextOptions<ShopDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ShopDbContext>>();

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<ShopDbContext>((provider, options) =>
                options.UseSqlite(
                    provider.GetRequiredService<DbConnection>()));
        });
    }
}