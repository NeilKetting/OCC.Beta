using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInventorySplitAndSupplierRefinements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0a049639-43b6-4c32-9c86-01f4a47f59de"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("10b35e76-b124-430a-8d9c-59d4babe453e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("1e57675b-1ce0-46b3-8187-1a8941f761db"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("24057d05-9b1c-4160-bf08-1cc4cd5045f1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2eed64d5-ed86-4101-925a-abc79b5b9095"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("33fa9102-4528-4bb3-95be-3167e2822950"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("788c1b74-32f4-445e-aba3-b1c103e89a1d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("b66fdd2d-cc1f-4643-8033-364c80752a67"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ba9fbbc1-b8a4-4485-adaf-772858ac8739"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("c78bfb9b-f1ce-4083-95db-412c22fb80fa"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d7bb6e51-80df-467d-b1ae-42f9c0bf29bf"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("e439dd11-73d7-46bc-9e62-12b8c6f32a6c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("efaf41aa-12a1-47f7-96d7-d14480b1c8ce"));

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "Suppliers",
                newName: "BankAccountNumber");

            migrationBuilder.AddColumn<int>(
                name: "Branch",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierAccountNumber",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "CptQuantity",
                table: "InventoryItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsStockItem",
                table: "InventoryItems",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "JhbQuantity",
                table: "InventoryItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("13a75da0-3966-4637-834d-e49e56b131dd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("2535c9b3-f6ec-457f-8f2e-01d1eaa3fe25"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("2c260c9b-bdba-4a76-a780-db23eb5aee68"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("3509c7c3-7333-4ce8-b475-345f99dc2c57"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("39c70497-26d3-4d54-aeeb-c0792a48faf1"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("4fc5cda3-22ab-4710-82f8-8ab4c0c46ceb"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("7f263717-a70d-4dd1-a89e-6a28a531671f"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("839b3101-cd8b-4de4-a074-009229de5e72"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("89797d4e-b78b-4147-ac22-6688716eeed8"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("8e7e31c9-74df-4e3e-9e0f-4a63f14fbb7b"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("94f550d4-249f-44c3-bdeb-7b46e175ece4"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("a3e0870b-fa0d-4b95-8f78-7e28b496b9c9"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("aba87122-5bfe-49a7-9c11-49d9a0278831"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("13a75da0-3966-4637-834d-e49e56b131dd"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2535c9b3-f6ec-457f-8f2e-01d1eaa3fe25"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2c260c9b-bdba-4a76-a780-db23eb5aee68"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("3509c7c3-7333-4ce8-b475-345f99dc2c57"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("39c70497-26d3-4d54-aeeb-c0792a48faf1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("4fc5cda3-22ab-4710-82f8-8ab4c0c46ceb"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7f263717-a70d-4dd1-a89e-6a28a531671f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("839b3101-cd8b-4de4-a074-009229de5e72"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("89797d4e-b78b-4147-ac22-6688716eeed8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("8e7e31c9-74df-4e3e-9e0f-4a63f14fbb7b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("94f550d4-249f-44c3-bdeb-7b46e175ece4"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("a3e0870b-fa0d-4b95-8f78-7e28b496b9c9"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("aba87122-5bfe-49a7-9c11-49d9a0278831"));

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SupplierAccountNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CptQuantity",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "IsStockItem",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "JhbQuantity",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "BankAccountNumber",
                table: "Suppliers",
                newName: "AccountNumber");

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("0a049639-43b6-4c32-9c86-01f4a47f59de"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("10b35e76-b124-430a-8d9c-59d4babe453e"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("1e57675b-1ce0-46b3-8187-1a8941f761db"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("24057d05-9b1c-4160-bf08-1cc4cd5045f1"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("2eed64d5-ed86-4101-925a-abc79b5b9095"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("33fa9102-4528-4bb3-95be-3167e2822950"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("788c1b74-32f4-445e-aba3-b1c103e89a1d"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("b66fdd2d-cc1f-4643-8033-364c80752a67"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("ba9fbbc1-b8a4-4485-adaf-772858ac8739"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("c78bfb9b-f1ce-4083-95db-412c22fb80fa"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("d7bb6e51-80df-467d-b1ae-42f9c0bf29bf"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("e439dd11-73d7-46bc-9e62-12b8c6f32a6c"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("efaf41aa-12a1-47f7-96d7-d14480b1c8ce"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" }
                });
        }
    }
}
