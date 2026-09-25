using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastaneApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryCity",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentToken",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentToken",
                table: "Orders",
                column: "PaymentToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentToken",
                table: "Orders");
        }
    }
}
