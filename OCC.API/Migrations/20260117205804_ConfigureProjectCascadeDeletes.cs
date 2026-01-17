using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureProjectCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_ProjectTaskId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_ProjectTaskId",
                table: "ProjectTasks");

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

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                table: "ProjectTasks");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectTaskId1",
                table: "TaskAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecords_ProjectId",
                table: "TimeRecords",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskId",
                table: "TaskComments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_ProjectTaskId1",
                table: "TaskAssignments",
                column: "ProjectTaskId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ParentId",
                table: "ProjectTasks",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProjectId",
                table: "Orders",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Projects_ProjectId",
                table: "Orders",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_ParentId",
                table: "ProjectTasks",
                column: "ParentId",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_ProjectTasks_ProjectTaskId1",
                table: "TaskAssignments",
                column: "ProjectTaskId1",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_ProjectTasks_TaskId",
                table: "TaskComments",
                column: "TaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeRecords_Projects_ProjectId",
                table: "TimeRecords",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Projects_ProjectId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_ParentId",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_ProjectTasks_ProjectTaskId1",
                table: "TaskAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_ProjectTasks_TaskId",
                table: "TaskComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeRecords_Projects_ProjectId",
                table: "TimeRecords");

            migrationBuilder.DropIndex(
                name: "IX_TimeRecords_ProjectId",
                table: "TimeRecords");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_TaskId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_ProjectTaskId1",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_ParentId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProjectId",
                table: "Orders");

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

            migrationBuilder.DropColumn(
                name: "ProjectTaskId1",
                table: "TaskAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectTaskId",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectTaskId",
                table: "ProjectTasks",
                column: "ProjectTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectTasks_ProjectTaskId",
                table: "ProjectTasks",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id");
        }
    }
}
