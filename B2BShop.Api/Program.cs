var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/products", () =>
{
    Product[] products =
    [
        new Product(1, "Mineral Water", 99),
        new Product(2, "Apple Juice", 249),
        new Product(3, "Coffee Beans", 1299)
    ];
    return products;
});

app.Run();

public record Product(int Id, string Name, int PriceIncents);
