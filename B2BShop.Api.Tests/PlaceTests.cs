using B2BShop.Api.Models;

namespace B2BShop.Api.Tests;

public class PlaceTests
{
    [Fact]
    public void Constructor_TrimsNameAndSetsNormalizedName()
    {
        // Act
        var place = new Place(" Bella Pizza Mitte ");

        // Assert
        Assert.Equal("Bella Pizza Mitte", place.Name);
        Assert.Equal("BELLA PIZZA MITTE", place.NormalizedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsNullOrWhiteSpaceName(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new Place(name!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_InvalidName_PreservesExistingName(string? newName)
    {
        // Arrange
        var place = new Place("Bella Pizza Mitte");
        var originalName = place.Name;
        var originalNormalizedName = place.NormalizedName;

        // Act
        Assert.ThrowsAny<ArgumentException>(
            () => place.Rename(newName!));

        // Assert
        Assert.Equal(originalName, place.Name);
        Assert.Equal(originalNormalizedName, place.NormalizedName);
    }

    [Fact]
    public void Rename_AcceptsValidName()
    {
        // Arrange
        var place = new Place("Bella Pizza Mitte");

        // Act
        place.Rename(" Bella Pizza Kreuzberg ");

        // Assert
        Assert.Equal("Bella Pizza Kreuzberg", place.Name);
        Assert.Equal("BELLA PIZZA KREUZBERG", place.NormalizedName);
    }
}