using B2BShop.Api.Data;
using Microsoft.EntityFrameworkCore;

using B2BShop.Api.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("ShopDatabase")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/products", async (ProductService  service) =>
    await service.GetAllAsync());

app.MapGet("/api/products/{id:int}", async (int id, ProductService service) =>
{
    var product = await service.GetByIdAsync(id);
    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);
});

app.MapGet("/api/carts/{id:guid}", async (Guid id, CartService service) =>
{
    var cartId = await service.GetByIdAsync(id);
    if (cartId is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(cartId);

});

app.MapPost("/api/carts", async (CartService service) =>
{
    var cartId = await service.CreateAsync();

    return Results.Created(
        $"/api/carts/{cartId}",
        new { id = cartId });
});

app.Run();
