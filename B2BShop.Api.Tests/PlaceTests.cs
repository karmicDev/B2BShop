using B2BShop.Api.Data;
using B2BShop.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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

    [Fact]
    public async Task CreatePlace_RejectsAlreadyExistingName()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        await factory.InitializeDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<ShopDbContext>();

        var place = new Place("Bella Pizza Mitte");
        context.Places.Add(place);
        await context.SaveChangesAsync();

        // Act
        var secondPlace = new Place(" bella pizza mitte ");
        context.Places.Add(secondPlace);

        // Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);

        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        Assert.Equal("IX_Places_NormalizedName", postgresException.ConstraintName);

        var count = await context.Places
            .AsNoTracking()
            .CountAsync(p => p.NormalizedName == "BELLA PIZZA MITTE");

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreatePlaces_WithDifferentNames_PersistsAll()
    {
        // Arrange
        await using var factory = new ShopApiFactory();
        await factory.StartDatabaseAsync();
        await factory.InitializeDatabaseAsync();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

        // Act
        context.Places.Add(new Place("Bella Pizza Mitte"));
        context.Places.Add(new Place("Bella Pizza Kreuzberg"));
        context.Places.Add(new Place("Bella Pizza Wedding"));
        await context.SaveChangesAsync();

        // Assert
        var places = await context.Places
            .AsNoTracking()
            .ToListAsync();
        Assert.Equal(3, places.Count);
        Assert.Contains(places, p => p.Name == "Bella Pizza Mitte");
        Assert.Contains(places, p => p.Name == "Bella Pizza Kreuzberg");
        Assert.Contains(places, p => p.Name == "Bella Pizza Wedding");
    }
}