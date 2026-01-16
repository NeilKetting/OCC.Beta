using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDismissals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0fd6b8fc-869b-4511-a725-74904ecfe8b3"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("11450551-57dd-4eb4-bf0c-525ff848d043"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("137c34a2-cea2-49e1-a7ce-d61a954bd012"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("36357386-d43a-4152-82a5-17708891c4d0"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("45154b97-7b2f-4661-9e78-f72d7f5cfe73"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("4e1a9bbd-4411-4bdc-8f25-2bdb7233e02f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("6d00513f-3ef2-488b-9b32-8fdb2d56ad67"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7067162d-f90c-4d3b-8328-1879a2653514"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7866616d-b3be-4c52-9cff-7d05d32fdf8d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("830b27f2-600f-46b5-8373-abbb19bd93c6"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("8a17e51e-f307-40a2-8080-a845621733e2"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("c6120d9c-ba96-46be-a3d1-9e83caf5dc43"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("dcd66a47-116a-49d3-b8a4-8a108b43fbfa"));

            migrationBuilder.CreateTable(
                name: "NotificationDismissals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DismissedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDismissals", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationDismissals");

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
                    { new Guid("0fd6b8fc-869b-4511-a725-74904ecfe8b3"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("11450551-57dd-4eb4-bf0c-525ff848d043"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("137c34a2-cea2-49e1-a7ce-d61a954bd012"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("36357386-d43a-4152-82a5-17708891c4d0"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("45154b97-7b2f-4661-9e78-f72d7f5cfe73"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("4e1a9bbd-4411-4bdc-8f25-2bdb7233e02f"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("6d00513f-3ef2-488b-9b32-8fdb2d56ad67"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("7067162d-f90c-4d3b-8328-1879a2653514"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("7866616d-b3be-4c52-9cff-7d05d32fdf8d"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("830b27f2-600f-46b5-8373-abbb19bd93c6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("8a17e51e-f307-40a2-8080-a845621733e2"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("c6120d9c-ba96-46be-a3d1-9e83caf5dc43"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("dcd66a47-116a-49d3-b8a4-8a108b43fbfa"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" }
                });
        }
    }
}
