using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class RepairProductIdentitySequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RestartSequence(
                name: "Products_Id_seq",
                startValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
