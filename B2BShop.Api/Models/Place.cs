using System.Diagnostics.CodeAnalysis;

namespace B2BShop.Api.Models;

public class Place
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;

    public PlaceType PlaceType { get; private set; }

    public Address? Address { get; private set; }
    
    public Place(string name)
    {
        Rename(name);
    }
    
    public Place(string name, Address address, PlaceType placeType)
    {
        Rename(name);
        ChangeAddress(address);
        PlaceType = placeType;
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        NormalizedName = Name.ToUpperInvariant();
    }

    [MemberNotNull(nameof(Address))]
    public void ChangeAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        Address = address;   
    }
}