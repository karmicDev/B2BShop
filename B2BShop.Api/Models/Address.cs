namespace B2BShop.Api.Models;

public class Address
{
    public string Street { get; private set; } = string.Empty;
    public string HouseNumber { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string CountryCode => "DE";
    public GeoLocation Location { get; private set; } = null!;

    // Used by EF Core when loading from the database
    private Address()
    {
    }

    public Address(string street, string houseNumber, string postalCode, string city, GeoLocation location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(houseNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentNullException.ThrowIfNull(location);
        
        var trimmedPostalCode = postalCode.Trim();

        if (trimmedPostalCode.Length != 5 ||
            !IsDigitsOnly(trimmedPostalCode))
        {
            throw new ArgumentException(
                "Postal code must contain exactly five digits.",
                nameof(postalCode));
        }
        
        Street = street.Trim();
        HouseNumber = houseNumber.Trim();
        PostalCode = trimmedPostalCode;
        City = city.Trim();
        Location = location;
    }
    
    private static bool IsDigitsOnly(string str)
    {
        foreach (char c in str)
        {
            if (c < '0' || c > '9')
                return false;
        }
        return true;
    }
}