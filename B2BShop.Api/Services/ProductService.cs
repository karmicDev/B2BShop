using B2BShop.Api.Models;
namespace B2BShop.Api.Services;

public class ProductService
{
    private readonly Product[] _products =
    [
        new Product(1, "Mineral Water", 99),
        new Product(2, "Apple Juice", 249),
        new Product(3, "Coffee Beans", 1299)
    ];

    public IEnumerable<Product> GetAll() => _products;
    
    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);
}