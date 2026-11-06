using B2BShop.Api.Data;
using Microsoft.EntityFrameworkCore;

using B2BShop.Api.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ProductService>();
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("ShopDatabase")));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/products", (ProductService service) => service.GetAll());

app.MapGet("/api/products/{id:int}", (int id, ProductService service) =>
{
    var product = service.GetById(id);
    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);
});

app.Run();
