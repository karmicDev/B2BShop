using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceAddressAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Places",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_HouseNumber",
                table: "Places",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Address_Location_Latitude",
                table: "Places",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Address_Location_Longitude",
                table: "Places",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "Places",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Places",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlaceType",
                table: "Places",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Address_HouseNumber",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Address_Location_Latitude",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Address_Location_Longitude",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "PlaceType",
                table: "Places");
        }
    }
}
