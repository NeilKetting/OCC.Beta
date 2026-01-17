using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureProjectCascadeDeletes_Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_ProjectTasks_ProjectTaskId1",
                table: "TaskAssignments");

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0027d17b-a7de-4d59-a050-3d432cb0bf45"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0639347a-0b0c-476f-885b-89c21abd5cb8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("07094e7a-aed3-4084-a549-409306ecca88"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("08e7a258-39a5-4a26-b92d-2f78d3181a96"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("1075f628-a0bd-4f77-ac88-388b86d31be1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("3a213c9a-93a3-431f-a010-d2f161d20399"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("411cde2d-ee0d-4210-9f43-0bae803d6974"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("73c26498-698e-41f3-a24c-c6c5269c852b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ae358506-cb47-4b0b-aeff-6e9704350c79"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("b54a7ae3-15b3-4f7a-81a9-c906c2834b5d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("c68d9c6e-041f-4ae4-8591-592624296676"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("edc1cc73-44dc-4734-a4b4-70578cca10e6"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f903b2f9-d7d3-449c-a47c-f76fb8db2be1"));

            migrationBuilder.RenameColumn(
                name: "ProjectTaskId1",
                table: "TaskAssignments",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAssignments_ProjectTaskId1",
                table: "TaskAssignments",
                newName: "IX_TaskAssignments_TaskId");

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("018848a2-10f2-4086-8885-4498928f367e"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("0ebed69a-6623-4456-9b44-813335cf8a5c"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("26192af2-d726-4c76-91df-2f898dddff9c"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("372cb7dc-64ee-497a-b40e-141694313164"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("6e22b3d6-d071-4d81-9eb8-f463e91674e1"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("73d4a7ed-8824-4dcf-a5ec-5fac16ff5798"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("7c756954-d9e8-4049-b320-3877ff53b27e"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("8e7ebfd0-822a-4928-b6c6-bf61ac9dfad4"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("98a30b5f-9c9e-4de1-9d11-f84fb9983d0e"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("b0b57d5c-9c6f-49d4-b12d-994f6ed16f99"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("d0d2e41f-3463-42c4-b630-82eb40728392"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("d417207d-8d8c-4653-939f-50ab4765a7bc"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("f6369c94-bd7e-4724-a35a-43664095d5ad"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_ProjectTasks_TaskId",
                table: "TaskAssignments",
                column: "TaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_ProjectTasks_TaskId",
                table: "TaskAssignments");

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("018848a2-10f2-4086-8885-4498928f367e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0ebed69a-6623-4456-9b44-813335cf8a5c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("26192af2-d726-4c76-91df-2f898dddff9c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("372cb7dc-64ee-497a-b40e-141694313164"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("6e22b3d6-d071-4d81-9eb8-f463e91674e1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("73d4a7ed-8824-4dcf-a5ec-5fac16ff5798"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7c756954-d9e8-4049-b320-3877ff53b27e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("8e7ebfd0-822a-4928-b6c6-bf61ac9dfad4"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("98a30b5f-9c9e-4de1-9d11-f84fb9983d0e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("b0b57d5c-9c6f-49d4-b12d-994f6ed16f99"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d0d2e41f-3463-42c4-b630-82eb40728392"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d417207d-8d8c-4653-939f-50ab4765a7bc"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f6369c94-bd7e-4724-a35a-43664095d5ad"));

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskAssignments",
                newName: "ProjectTaskId1");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAssignments_TaskId",
                table: "TaskAssignments",
                newName: "IX_TaskAssignments_ProjectTaskId1");

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("0027d17b-a7de-4d59-a050-3d432cb0bf45"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("0639347a-0b0c-476f-885b-89c21abd5cb8"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("07094e7a-aed3-4084-a549-409306ecca88"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("08e7a258-39a5-4a26-b92d-2f78d3181a96"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("1075f628-a0bd-4f77-ac88-388b86d31be1"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("3a213c9a-93a3-431f-a010-d2f161d20399"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("411cde2d-ee0d-4210-9f43-0bae803d6974"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("73c26498-698e-41f3-a24c-c6c5269c852b"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("ae358506-cb47-4b0b-aeff-6e9704350c79"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("b54a7ae3-15b3-4f7a-81a9-c906c2834b5d"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("c68d9c6e-041f-4ae4-8591-592624296676"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("edc1cc73-44dc-4734-a4b4-70578cca10e6"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("f903b2f9-d7d3-449c-a47c-f76fb8db2be1"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_ProjectTasks_ProjectTaskId1",
                table: "TaskAssignments",
                column: "ProjectTaskId1",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
