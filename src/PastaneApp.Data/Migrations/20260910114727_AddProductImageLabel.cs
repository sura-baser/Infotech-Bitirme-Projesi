using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastaneApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImageLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "ProductImages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "ProductImages");
        }
    }
}
