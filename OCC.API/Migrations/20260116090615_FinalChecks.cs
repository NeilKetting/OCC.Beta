using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class FinalChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0dd577c3-428d-4abc-983c-0aa6a0999b9e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("10ad7ab2-e8af-4f66-a081-9b339d654d7c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("15262a35-be16-498d-ac16-b29569a229eb"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("44430c30-4bdc-4cfb-92cf-b808414daa88"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("50f6d9f0-04d8-4436-ac15-d48ceb31465f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("50fac1e3-2075-4ff1-9dcb-746d67a7464b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("62e6eca3-3cfa-4322-8a5f-8e63a358d3b9"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("adb5fee4-67a5-4b4f-aecc-892fb7449b6d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("be44dc94-1653-4486-be48-3f77a052b7a3"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("e593f931-aa76-47ec-aa3f-77c299c5fa91"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("edde7053-9109-4faf-81ee-8678b92a511f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f7a1432d-7898-4830-95e4-0a28be072973"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f8db2aea-2ea8-43e9-9460-9b306b47b26b"));

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("02df5455-fd85-465a-94b0-3e2afc6c5147"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("10f8d366-9d16-4385-8ec1-871fc54640c1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("18f5c93a-cf8a-44c5-ad74-aaf1518ebcf8"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("49ca9b35-a6b6-4498-8fa7-41c90281de50"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("5fba8cf1-89df-47a8-a909-d3bb84d2728a"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("789a5d55-fe1b-4683-9901-08c8d7f750f7"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("9ef1597f-298d-425b-b869-ecc39f3863e3"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("d9bf990a-cea4-4e9f-8642-a7e446c80f46"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("e4a81a4b-a8ac-46f0-9335-89cf619d84f8"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("e71fab50-0bcf-44ee-b1fb-6a4ef62899e1"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("ed02e768-3cab-4b75-b4d6-abb6fe794b29"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("f3b46995-5193-484d-80d7-5750d5263554"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("fe2b0451-c692-482b-93c4-b0d1e39e91fd"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("02df5455-fd85-465a-94b0-3e2afc6c5147"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("10f8d366-9d16-4385-8ec1-871fc54640c1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("18f5c93a-cf8a-44c5-ad74-aaf1518ebcf8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("49ca9b35-a6b6-4498-8fa7-41c90281de50"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5fba8cf1-89df-47a8-a909-d3bb84d2728a"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("789a5d55-fe1b-4683-9901-08c8d7f750f7"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("9ef1597f-298d-425b-b869-ecc39f3863e3"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d9bf990a-cea4-4e9f-8642-a7e446c80f46"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("e4a81a4b-a8ac-46f0-9335-89cf619d84f8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("e71fab50-0bcf-44ee-b1fb-6a4ef62899e1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ed02e768-3cab-4b75-b4d6-abb6fe794b29"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f3b46995-5193-484d-80d7-5750d5263554"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("fe2b0451-c692-482b-93c4-b0d1e39e91fd"));

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("0dd577c3-428d-4abc-983c-0aa6a0999b9e"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("10ad7ab2-e8af-4f66-a081-9b339d654d7c"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("15262a35-be16-498d-ac16-b29569a229eb"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("44430c30-4bdc-4cfb-92cf-b808414daa88"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("50f6d9f0-04d8-4436-ac15-d48ceb31465f"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("50fac1e3-2075-4ff1-9dcb-746d67a7464b"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("62e6eca3-3cfa-4322-8a5f-8e63a358d3b9"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("adb5fee4-67a5-4b4f-aecc-892fb7449b6d"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("be44dc94-1653-4486-be48-3f77a052b7a3"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("e593f931-aa76-47ec-aa3f-77c299c5fa91"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("edde7053-9109-4faf-81ee-8678b92a511f"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("f7a1432d-7898-4830-95e4-0a28be072973"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("f8db2aea-2ea8-43e9-9460-9b306b47b26b"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" }
                });
        }
    }
}
