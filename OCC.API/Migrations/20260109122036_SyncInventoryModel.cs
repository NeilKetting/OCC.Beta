using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncInventoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("143d8b0c-cd0e-42de-9880-b4b8237e672a"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2e78b119-f31c-421c-a9e5-b420be8d1cbb"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("31be3e2c-bf88-4306-a8cb-09f062ebc40b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("404571ee-46aa-440e-aeff-d71a2cd69b8f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("4adccde9-4f7d-4ee8-9d81-5001caf95476"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("584822c3-1a4d-451c-968e-dad89e96dc6c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("679ff7b9-6665-45cb-b2bc-5d719fe13528"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("70c061c3-df30-4174-8e8a-0aea850487e2"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7b90f15c-245c-4b97-a36d-9f8142da5158"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("96e79440-f4a8-453f-9a18-ae114d0f5d10"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ce05d56f-9972-4479-a5a7-0ade5f00664b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d9b909ce-4224-4ac4-ae74-a8dd05713453"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ee7e6d38-b89c-4f72-bacd-0e4a6f0d81d2"));

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("010c68ed-310d-4005-9cc3-d72a8b58d410"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("1fde9626-576a-48cf-8aa0-d99ed6fcf30f"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("2bdeb9f0-1396-4b21-9305-3e58e94d53b5"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("3d0bf74e-20b7-4220-acd2-596babd2ee94"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("72eed25a-ae35-4dc4-9eea-04b97c5dd8d8"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("7b9bee09-1aa7-408d-a02c-0e51af42492d"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("7c1864de-980f-4cbf-a3b5-70a7550193c8"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("88268e90-6f0b-4b5c-abb6-67b672ecdb04"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("8c2a17da-a22b-4d25-991e-9e7a86860d81"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("a6c57454-0d93-4c23-aba9-42c0546b17f4"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("d80da547-6df8-43d1-a13b-fc7c1dff0f09"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("f86d7965-9f31-4ee6-8ec1-39c1eca2ff0e"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("fdff892e-d223-46f4-be82-32f88c0d025e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("010c68ed-310d-4005-9cc3-d72a8b58d410"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("1fde9626-576a-48cf-8aa0-d99ed6fcf30f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2bdeb9f0-1396-4b21-9305-3e58e94d53b5"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("3d0bf74e-20b7-4220-acd2-596babd2ee94"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("72eed25a-ae35-4dc4-9eea-04b97c5dd8d8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7b9bee09-1aa7-408d-a02c-0e51af42492d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7c1864de-980f-4cbf-a3b5-70a7550193c8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("88268e90-6f0b-4b5c-abb6-67b672ecdb04"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("8c2a17da-a22b-4d25-991e-9e7a86860d81"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("a6c57454-0d93-4c23-aba9-42c0546b17f4"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d80da547-6df8-43d1-a13b-fc7c1dff0f09"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f86d7965-9f31-4ee6-8ec1-39c1eca2ff0e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("fdff892e-d223-46f4-be82-32f88c0d025e"));

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("143d8b0c-cd0e-42de-9880-b4b8237e672a"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("2e78b119-f31c-421c-a9e5-b420be8d1cbb"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("31be3e2c-bf88-4306-a8cb-09f062ebc40b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("404571ee-46aa-440e-aeff-d71a2cd69b8f"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("4adccde9-4f7d-4ee8-9d81-5001caf95476"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("584822c3-1a4d-451c-968e-dad89e96dc6c"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("679ff7b9-6665-45cb-b2bc-5d719fe13528"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("70c061c3-df30-4174-8e8a-0aea850487e2"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("7b90f15c-245c-4b97-a36d-9f8142da5158"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("96e79440-f4a8-453f-9a18-ae114d0f5d10"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("ce05d56f-9972-4479-a5a7-0ade5f00664b"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("d9b909ce-4224-4ac4-ae74-a8dd05713453"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("ee7e6d38-b89c-4f72-bacd-0e4a6f0d81d2"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" }
                });
        }
    }
}
