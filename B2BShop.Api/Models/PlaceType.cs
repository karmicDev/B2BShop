namespace B2BShop.Api.Models;

// Stored as text in the database (see ShopDbContext), so the order of the
// values does not matter, but renaming a value requires a data migration.
public enum PlaceType
{
    Restaurant,
    Groceries,
    Bakery,
    Shops,
    HealthAndBeauty,
    Electronics,
    Convenience,
    Sweets,
    Alcohol,
    Flowers,
    CoffeeAndTea,
    ToysAndGames,
    Specialty,
    HomeAndKitchen
}