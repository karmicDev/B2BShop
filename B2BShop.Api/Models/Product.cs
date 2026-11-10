namespace B2BShop.Api.Models;

public record Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PriceInCents { get; set; }

    public Product()
    {
    }

    public Product(int id, string name, int priceInCents)
    {
        Id = id;
        Name = name;
        PriceInCents = priceInCents;
    }
    
    public Product(string name, int priceInCents)
    {
        Name = name;
        PriceInCents = priceInCents;
    }
}