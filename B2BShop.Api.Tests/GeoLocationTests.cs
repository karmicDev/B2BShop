using B2BShop.Api.Models;
namespace B2BShop.Api.Tests;

public class GeoLocationTests
{
    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(0, 0)]
    [InlineData(52.52, 13.405)]
    public void CreateGeoLocation_ValidLatitudeAndLongitude_GetsCorrectResult(double latitude, double longitude)
    {
        // Act
        var geoLocation = new GeoLocation(latitude, longitude);

        // Assert
        Assert.Equal(latitude, geoLocation.Latitude);
        Assert.Equal(longitude, geoLocation.Longitude);
    }

    [Theory]
    [InlineData(-90.001, 0, "latitude")]
    [InlineData(90.001, 0, "latitude")]
    [InlineData(0, -180.001, "longitude")]
    [InlineData(0, 180.001, "longitude")]
    [InlineData(double.NaN, 0, "latitude")]
    [InlineData(0, double.NaN, "longitude")]
    [InlineData(double.PositiveInfinity, 0, "latitude")]
    [InlineData(0, double.NegativeInfinity, "longitude")]
    public void CreateGeoLocation_InvalidCoordinates_ThrowsForInvalidParameter(
        double latitude,
        double longitude,
        string expectedParamName)
    {
        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GeoLocation(latitude, longitude));

        // Assert
        Assert.Equal(expectedParamName, exception.ParamName);
    }
}