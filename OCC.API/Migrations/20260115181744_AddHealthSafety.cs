using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthSafety : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("25717962-c707-41e5-9ec1-8c063c097a49"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("26a4cef4-cd46-4289-b791-179d8437af2f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("26ada812-05a9-4d49-a5b3-f89e0499ae3d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("51c39ac9-2f49-4e4a-b3e0-8795c7777e8e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("562d8581-6929-430a-9b75-83b6034fbf49"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("684b7c7e-93f9-4b90-aab8-8f942a5dc106"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("98adb011-4ab7-4045-b612-6fbb44a4e19d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("a56007ad-c69f-4200-ba20-0f290ef61ec9"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("be589c2b-7174-47a0-94e4-589d3b7dcd9f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("c1d72294-57f9-4faf-aa2b-f67aad86ee82"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("c7aab64f-f0f6-47c0-8b87-320605b3c05f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d0c0be26-6260-4e55-8341-5b204cc4a489"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("e5392338-4688-4491-8375-d274c459020e"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AttendanceRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "HoursWorked",
                table: "AttendanceRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AttendanceRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AttendanceRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HseqAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScopeOfWorks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteManager = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteSupervisor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HseqConsultant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuditNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NonConformance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImmediateAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CloseOutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HseqAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HseqSafeHourRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SafeWorkHours = table.Column<double>(type: "float", nullable: false),
                    IncidentReported = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NearMisses = table.Column<int>(type: "int", nullable: false),
                    RootCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectiveActions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HseqSafeHourRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HseqTrainingRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrainingTopic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCompleted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Trainer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificateUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryWarningDays = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HseqTrainingRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InvestigatorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RootCause = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HseqAuditComplianceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegulationReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HseqAuditComplianceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HseqAuditComplianceItems_HseqAudits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "HseqAudits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HseqAuditNonComplianceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegulationReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HseqAuditNonComplianceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HseqAuditNonComplianceItems_HseqAudits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "HseqAudits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HseqAuditSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PossibleScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HseqAuditSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HseqAuditSections_HseqAudits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "HseqAudits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Base64Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentPhotos_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("0267e363-e878-48a1-93b3-900d04e4e1d9"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("23b66745-4568-47d9-a809-d08c7e09f0f0"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("5e97dbc4-a07a-4eef-ae8c-ac0a40a7d495"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("5fc49eb3-bcf9-40fa-8ee9-fe34c254b107"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("606f4100-a0d9-4e69-8be2-1a1f115267cd"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("952ec8e8-8c95-42c4-822d-7e185f234113"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("b0560205-9776-4527-9c47-719cbefeaabc"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("b2a13e2d-42bc-4f20-b104-b780d72ec7e4"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("ce44359b-cca9-4172-b027-2f70d2cd8bf9"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("f5fe97b6-bf32-411e-bc90-9509bab53dc0"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("f6197f42-adc3-478a-825b-559ec791f940"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("fa7eae29-e381-4aec-8493-1d8ec86f08f8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("feaa09f0-de31-4fcc-9ec3-c2eb5ebfeecf"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_HseqAuditComplianceItems_AuditId",
                table: "HseqAuditComplianceItems",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_HseqAuditNonComplianceItems_AuditId",
                table: "HseqAuditNonComplianceItems",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_HseqAuditSections_AuditId",
                table: "HseqAuditSections",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentPhotos_IncidentId",
                table: "IncidentPhotos",
                column: "IncidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HseqAuditComplianceItems");

            migrationBuilder.DropTable(
                name: "HseqAuditNonComplianceItems");

            migrationBuilder.DropTable(
                name: "HseqAuditSections");

            migrationBuilder.DropTable(
                name: "HseqSafeHourRecords");

            migrationBuilder.DropTable(
                name: "HseqTrainingRecords");

            migrationBuilder.DropTable(
                name: "IncidentPhotos");

            migrationBuilder.DropTable(
                name: "HseqAudits");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0267e363-e878-48a1-93b3-900d04e4e1d9"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("23b66745-4568-47d9-a809-d08c7e09f0f0"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5e97dbc4-a07a-4eef-ae8c-ac0a40a7d495"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5fc49eb3-bcf9-40fa-8ee9-fe34c254b107"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("606f4100-a0d9-4e69-8be2-1a1f115267cd"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("952ec8e8-8c95-42c4-822d-7e185f234113"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("b0560205-9776-4527-9c47-719cbefeaabc"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("b2a13e2d-42bc-4f20-b104-b780d72ec7e4"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ce44359b-cca9-4172-b027-2f70d2cd8bf9"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f5fe97b6-bf32-411e-bc90-9509bab53dc0"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f6197f42-adc3-478a-825b-559ec791f940"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("fa7eae29-e381-4aec-8493-1d8ec86f08f8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("feaa09f0-de31-4fcc-9ec3-c2eb5ebfeecf"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "HoursWorked",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AttendanceRecords");

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("25717962-c707-41e5-9ec1-8c063c097a49"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("26a4cef4-cd46-4289-b791-179d8437af2f"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("26ada812-05a9-4d49-a5b3-f89e0499ae3d"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("51c39ac9-2f49-4e4a-b3e0-8795c7777e8e"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("562d8581-6929-430a-9b75-83b6034fbf49"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("684b7c7e-93f9-4b90-aab8-8f942a5dc106"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("98adb011-4ab7-4045-b612-6fbb44a4e19d"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("a56007ad-c69f-4200-ba20-0f290ef61ec9"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("be589c2b-7174-47a0-94e4-589d3b7dcd9f"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("c1d72294-57f9-4faf-aa2b-f67aad86ee82"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("c7aab64f-f0f6-47c0-8b87-320605b3c05f"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("d0c0be26-6260-4e55-8341-5b204cc4a489"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("e5392338-4688-4491-8375-d274c459020e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" }
                });
        }
    }
}
