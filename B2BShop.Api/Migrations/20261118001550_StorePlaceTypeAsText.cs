using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BShop.Api.Migrations
{
    /// <summary>
    /// Stores PlaceType as its name instead of its position in the enum, so
    /// adding or reordering enum values no longer changes existing data.
    /// The removed value Bar (stored as 1) becomes Restaurant.
    /// </summary>
    public partial class StorePlaceTypeAsText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The integer column had DEFAULT 0, which has no meaning as text.
            // Unknown numbers map to NULL and fail the NOT NULL constraint.
            migrationBuilder.Sql("""
                ALTER TABLE "Places" ALTER COLUMN "PlaceType" DROP DEFAULT;
                ALTER TABLE "Places" ALTER COLUMN "PlaceType" TYPE text
                USING CASE "PlaceType"
                    WHEN 0 THEN 'Restaurant'
                    WHEN 1 THEN 'Restaurant'
                    WHEN 2 THEN 'Groceries'
                    WHEN 3 THEN 'Bakery'
                    WHEN 4 THEN 'Shops'
                    WHEN 5 THEN 'HealthAndBeauty'
                    WHEN 6 THEN 'Electronics'
                    WHEN 7 THEN 'Convenience'
                    WHEN 8 THEN 'Sweets'
                    WHEN 9 THEN 'Alcohol'
                    WHEN 10 THEN 'Flowers'
                    WHEN 11 THEN 'CoffeeAndTea'
                    WHEN 12 THEN 'ToysAndGames'
                    WHEN 13 THEN 'Specialty'
                    WHEN 14 THEN 'HomeAndKitchen'
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Uses the old enum positions; Bar cannot be restored.
            migrationBuilder.Sql("""
                ALTER TABLE "Places" ALTER COLUMN "PlaceType" TYPE integer
                USING CASE "PlaceType"
                    WHEN 'Restaurant' THEN 0
                    WHEN 'Groceries' THEN 2
                    WHEN 'Bakery' THEN 3
                    WHEN 'Shops' THEN 4
                    WHEN 'HealthAndBeauty' THEN 5
                    WHEN 'Electronics' THEN 6
                    WHEN 'Convenience' THEN 7
                    WHEN 'Sweets' THEN 8
                    WHEN 'Alcohol' THEN 9
                    WHEN 'Flowers' THEN 10
                    WHEN 'CoffeeAndTea' THEN 11
                    WHEN 'ToysAndGames' THEN 12
                    WHEN 'Specialty' THEN 13
                    WHEN 'HomeAndKitchen' THEN 14
                END;
                ALTER TABLE "Places" ALTER COLUMN "PlaceType" SET DEFAULT 0;
                """);
        }
    }
}
