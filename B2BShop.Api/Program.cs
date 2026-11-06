var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
Product[] products =
[
    new Product(1, "Mineral Water", 99),
    new Product(2, "Apple Juice", 249),
    new Product(3, "Coffee Beans", 1299)
];

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/products", () => products);

app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);
});

app.Run();

public record Product(int Id, string Name, int PriceInCents);
