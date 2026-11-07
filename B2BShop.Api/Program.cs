using B2BShop.Api.Data;
using Microsoft.EntityFrameworkCore;
using B2BShop.Api.Contracts;

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

app.MapPut("/api/carts/{cartId:guid}/items/{productId:int}",
    async (
        Guid cartId,
        int productId,
        SetCartItemRequest request,
        CartService service) =>
    {
        var result = await service.SetItemAsync(
            cartId, productId, request.Quantity);

        return result switch
        {
            SetCartItemResult.Success => Results.NoContent(),
            SetCartItemResult.InvalidQuantity =>
                Results.BadRequest(new { error = "Quantity must be between 1 and 99." }),
            SetCartItemResult.CartNotFound =>
                Results.NotFound(new { error = "Cart not found." }),
            SetCartItemResult.ProductNotFound =>
                Results.NotFound(new { error = "Product not found." }),
            SetCartItemResult.Conflict =>
                Results.Conflict(new { error = "The cart is busy. Please retry the request." }),
            _ => Results.StatusCode(500)
        };
    });

app.MapPost("/api/carts/{cartId:guid}/items/{productId:int}/increment",
    async (
        Guid cartId,
        int productId,
        CartService cartService) =>
    {
        var result = await cartService.IncrementItemAsync(cartId, productId);
        return result switch
        {
            SetCartItemResult.Success =>
                Results.NoContent(),

            SetCartItemResult.InvalidQuantity =>
                Results.BadRequest(new
                {
                    error = "Quantity must be between 1 and 99."
                }),

            SetCartItemResult.CartNotFound =>
                Results.NotFound(new { error = "Cart not found." }),

            SetCartItemResult.ProductNotFound =>
                Results.NotFound(new { error = "Product not found." }),

            SetCartItemResult.Conflict =>
                Results.Conflict(new
                {
                    error = "The cart is busy. Please retry the request."
                }),

            _ => Results.StatusCode(500)
        };
    });

app.MapPost("/api/carts/{cartId:guid}/items/{productId:int}/decrement",
    async (
        Guid cartId,
        int productId,
        CartService cartService) =>
    {
        var result = await cartService.DecrementItemAsync(cartId, productId);
        return result switch
        {
            SetCartItemResult.Success =>
                Results.NoContent(),

            SetCartItemResult.InvalidQuantity =>
                Results.BadRequest(new
                {
                    error = "Quantity must be between 1 and 99."
                }),

            SetCartItemResult.CartNotFound =>
                Results.NotFound(new { error = "Cart not found." }),

            SetCartItemResult.ProductNotFound =>
                Results.NotFound(new { error = "Product not found." }),

            SetCartItemResult.Conflict =>
                Results.Conflict(new
                {
                    error = "The cart is busy. Please retry the request."
                }),

            _ => Results.StatusCode(500)
        };
    });

app.MapDelete("/api/carts/{cartId:guid}/items/{productId:int}",
    async (
        Guid cartId,
        int productId,
        CartService service) =>
    {
        var result = await service.RemoveItemAsync(cartId, productId);

        return result switch
        {
            SetCartItemResult.Success => Results.NoContent(),

            SetCartItemResult.CartNotFound => Results.NotFound(new { error = "Cart not found." }),

            SetCartItemResult.ProductNotFound => Results.NotFound(new { error = "Product not found." }),

            SetCartItemResult.Conflict => Results.Conflict(new
            {
                error = "The cart is busy. Please retry the request."
            }),

            _ => Results.StatusCode(500)
        };
    });

app.Run();