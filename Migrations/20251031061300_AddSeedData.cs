using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageUrl", "IsAvailable", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Món chính", new DateTime(2025, 10, 31, 13, 13, 0, 127, DateTimeKind.Local).AddTicks(1040), "Phở bò truyền thống với nước dùng đậm đà", "/images/pho-bo.jpg", true, "Phở Bò", 65000m },
                    { 2, "Món chính", new DateTime(2025, 10, 31, 13, 13, 0, 127, DateTimeKind.Local).AddTicks(2490), "Bún chả Hà Nội với thịt nướng thơm ngon", "/images/bun-cha.jpg", true, "Bún Chả", 55000m },
                    { 3, "Khai vị", new DateTime(2025, 10, 31, 13, 13, 0, 127, DateTimeKind.Local).AddTicks(2500), "Gỏi cuốn tôm thịt tươi ngon", "/images/goi-cuon.jpg", true, "Gỏi Cuốn", 35000m },
                    { 4, "Món chính", new DateTime(2025, 10, 31, 13, 13, 0, 127, DateTimeKind.Local).AddTicks(2500), "Chả cá truyền thống với thì là và hành", "/images/cha-ca.jpg", true, "Chả Cá Lã Vọng", 85000m },
                    { 5, "Món nhẹ", new DateTime(2025, 10, 31, 13, 13, 0, 127, DateTimeKind.Local).AddTicks(2510), "Bánh mì thịt nguội với rau củ tươi", "/images/banh-mi.jpg", true, "Bánh Mì", 25000m },
                    { 6, "Đồ uống", new DateTime(2025, 10, 31, 13, 13, 0, 127, DateTimeKind.Local).AddTicks(2510), "Cà phê sữa đá truyền thống", "/images/ca-phe.jpg", true, "Cà Phê Sữa Đá", 20000m }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginAt", "Password", "Role", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@restaurant.com", "Administrator", true, null, "admin123", 2, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
