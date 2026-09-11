using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastaneApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductHighlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Highlights",
                table: "Products",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Highlights",
                table: "Products");
        }
    }
}
