namespace B2BShop.Api.Models;

public class GeoLocation
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public GeoLocation(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude < -90 || latitude > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                latitude,
                "Latitude must be between -90 and 90.");
        }
        if (!double.IsFinite(longitude) || longitude < -180.0 || longitude > 180.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                longitude,
                "Longitude must be between -180.0 and 180.");
        }
        Latitude = latitude;
        Longitude = longitude;
    }
}