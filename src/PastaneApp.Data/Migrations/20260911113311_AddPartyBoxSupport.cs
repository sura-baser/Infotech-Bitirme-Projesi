using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastaneApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartyBoxSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomizationNotes",
                table: "OrderDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOnHome",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomizationNotes",
                table: "CartItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomizationNotes",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ShowOnHome",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CustomizationNotes",
                table: "CartItems");
        }
    }
}
