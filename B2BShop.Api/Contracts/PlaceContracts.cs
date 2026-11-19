using B2BShop.Api.Models;

namespace B2BShop.Api.Contracts;

public record CreateAddressRequest(
    string Street, string HouseNumber, string PostalCode, string City,
    double Latitude, double Longitude);

public record CreatePlaceRequest(
    string Name, PlaceType Type, CreateAddressRequest? Address);

public record AddressResponse(
    string Street, string HouseNumber, string PostalCode, string City,
    string CountryCode, double Latitude, double Longitude);

public record PlaceResponse(
    int Id,
    string Name,
    PlaceType Type,
    AddressResponse? Address)
{
    public static PlaceResponse From(Place place)
    {
        // Places created without an address (new Place(name)) return null here.
        var address = place.Address is null
            ? null
            : new AddressResponse(
                place.Address.Street,
                place.Address.HouseNumber,
                place.Address.PostalCode,
                place.Address.City,
                place.Address.CountryCode,
                place.Address.Location.Latitude,
                place.Address.Location.Longitude);

        return new PlaceResponse(place.Id, place.Name, place.PlaceType, address);
    }
}
