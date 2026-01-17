using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class FixPlannedDurationOverflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("16d020de-01eb-48b4-b496-903e1103ec6c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("26e4d411-3bcd-4c0c-afb5-dfad207c8b4c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("4c366f40-24dd-4423-b6f5-a1ce1c62e9db"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("4c4d1570-3cae-4413-8e96-ccfedb2922c3"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5c00cac5-4512-4e89-9706-d723497cd8c1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5dfe346c-d414-4375-bccb-84fc085d4544"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5e05c578-0ad9-4156-847f-e9e074811e62"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("6091d74a-53af-45a2-a372-1c8e2136a056"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7f1554c6-53a9-4b0c-9807-60eae4fd4ab3"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("899180a1-b96d-4a5c-802c-a38eb5c4e892"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d0410b6d-235d-4046-a2ee-baced981696e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d75c6961-c88b-4943-b804-0c886dd57a78"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f5f9d6ef-ed7a-4a0b-84a5-cd4f3f08cffe"));

            migrationBuilder.AlterColumn<long>(
                name: "PlanedDurationHours",
                table: "ProjectTasks",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ActualDuration",
                table: "ProjectTasks",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("0a2a85b6-8095-4410-bf28-281ced938a9d"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("270260a8-abe6-492e-a1fb-5febf9f022dd"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("275cb009-eac0-4189-bae1-f1d9530596c4"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("2aba20f8-7eac-4fa7-a789-93833d2597b6"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("303f6aa6-02af-4d47-ac8f-af0cb06250a5"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("51d1b85a-2409-411b-9cce-b7c5a3c2ada8"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("61474af0-9c39-408d-991c-384abb9cc2fb"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("81901f5c-d69b-4607-bfcb-b889f4f6d248"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("95db2b43-831b-4c04-8fe0-94a0e7639fab"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("d1771d51-5b59-4e57-92fc-4bd5be77d8f9"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("df1f12c0-93a4-4496-89ff-5e1ef3b17b77"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("eb1e6c7b-0e13-4155-a685-2df08f559a15"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("ebd0fb7f-662d-4662-acf6-b757608c5acc"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0a2a85b6-8095-4410-bf28-281ced938a9d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("270260a8-abe6-492e-a1fb-5febf9f022dd"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("275cb009-eac0-4189-bae1-f1d9530596c4"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2aba20f8-7eac-4fa7-a789-93833d2597b6"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("303f6aa6-02af-4d47-ac8f-af0cb06250a5"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("51d1b85a-2409-411b-9cce-b7c5a3c2ada8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("61474af0-9c39-408d-991c-384abb9cc2fb"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("81901f5c-d69b-4607-bfcb-b889f4f6d248"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("95db2b43-831b-4c04-8fe0-94a0e7639fab"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d1771d51-5b59-4e57-92fc-4bd5be77d8f9"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("df1f12c0-93a4-4496-89ff-5e1ef3b17b77"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("eb1e6c7b-0e13-4155-a685-2df08f559a15"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ebd0fb7f-662d-4662-acf6-b757608c5acc"));

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "PlanedDurationHours",
                table: "ProjectTasks",
                type: "time",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "ActualDuration",
                table: "ProjectTasks",
                type: "time",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("16d020de-01eb-48b4-b496-903e1103ec6c"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("26e4d411-3bcd-4c0c-afb5-dfad207c8b4c"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("4c366f40-24dd-4423-b6f5-a1ce1c62e9db"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("4c4d1570-3cae-4413-8e96-ccfedb2922c3"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("5c00cac5-4512-4e89-9706-d723497cd8c1"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("5dfe346c-d414-4375-bccb-84fc085d4544"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("5e05c578-0ad9-4156-847f-e9e074811e62"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("6091d74a-53af-45a2-a372-1c8e2136a056"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("7f1554c6-53a9-4b0c-9807-60eae4fd4ab3"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("899180a1-b96d-4a5c-802c-a38eb5c4e892"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("d0410b6d-235d-4046-a2ee-baced981696e"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("d75c6961-c88b-4943-b804-0c886dd57a78"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("f5f9d6ef-ed7a-4a0b-84a5-cd4f3f08cffe"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" }
                });
        }
    }
}
