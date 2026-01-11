using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCC.Client.Migrations
{
    /// <inheritdoc />
    public partial class AddPermitNumberToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "HourlyRate",
            //     table: "Users");

            // migrationBuilder.DropColumn(
            //     name: "Language",
            //     table: "Users");

            // migrationBuilder.AddColumn<bool>(
            //     name: "IsApproved",
            //     table: "Users",
            //     type: "bit",
            //     nullable: false,
            //     defaultValue: false);

            // migrationBuilder.AddColumn<bool>(
            //     name: "IsEmailVerified",
            //     table: "Users",
            //     type: "bit",
            //     nullable: false,
            //     defaultValue: false);

            // migrationBuilder.AddColumn<string>(
            //     name: "AccountNumber",
            //     table: "Employees",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "AccountType",
            //     table: "Employees",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<double>(
            //     name: "AnnualLeaveBalance",
            //     table: "Employees",
            //     type: "float",
            //     nullable: false,
            //     defaultValue: 0.0);

            // migrationBuilder.AddColumn<string>(
            //     name: "BankName",
            //     table: "Employees",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Branch",
            //     table: "Employees",
            //     type: "nvarchar(max)",
            //     nullable: false,
            //     defaultValue: "");

            // migrationBuilder.AddColumn<string>(
            //     name: "BranchCode",
            //     table: "Employees",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<double>(
            //     name: "LeaveBalance",
            //     table: "Employees",
            //     type: "float",
            //     nullable: false,
            //     defaultValue: 0.0);

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "LeaveCycleStartDate",
            //     table: "Employees",
            //     type: "datetime2",
            //     nullable: true);

            // migrationBuilder.AddColumn<Guid>(
            //     name: "LinkedUserId",
            //     table: "Employees",
            //     type: "uniqueidentifier",
            //     nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermitNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "RateType",
            //     table: "Employees",
            //     type: "int",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.AddColumn<TimeSpan>(
            //     name: "ShiftEndTime",
            //     table: "Employees",
            //     type: "time",
            //     nullable: true);

            // migrationBuilder.AddColumn<TimeSpan>(
            //     name: "ShiftStartTime",
            //     table: "Employees",
            //     type: "time",
            //     nullable: true);

            // migrationBuilder.AddColumn<double>(
            //     name: "SickLeaveBalance",
            //     table: "Employees",
            //     type: "float",
            //     nullable: false,
            //     defaultValue: 0.0);

            // migrationBuilder.AddColumn<int>(
            //     name: "Status",
            //     table: "Employees",
            //     type: "int",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.AddColumn<string>(
            //     name: "TaxNumber",
            //     table: "Employees",
            //     type: "nvarchar(max)",
            //     nullable: false,
            //     defaultValue: "");

            // migrationBuilder.AddColumn<string>(
            //     name: "Branch",
            //     table: "AttendanceRecords",
            //     type: "nvarchar(max)",
            //     nullable: false,
            //     defaultValue: "");

            // migrationBuilder.AddColumn<decimal>(
            //     name: "CachedHourlyRate",
            //     table: "AttendanceRecords",
            //     type: "decimal(18,2)",
            //     nullable: true);

            // migrationBuilder.AddColumn<TimeSpan>(
            //     name: "ClockInTime",
            //     table: "AttendanceRecords",
            //     type: "time",
            //     nullable: true);

            // migrationBuilder.CreateTable(
            //     name: "Teams",
            //     columns: table => new
            //     {
            //         Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //         Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         LeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Teams", x => x.Id);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "TeamMembers",
            //     columns: table => new
            //     {
            //         Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //         TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //         EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //         DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_TeamMembers", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_TeamMembers_Teams_TeamId",
            //             column: x => x.TeamId,
            //             principalTable: "Teams",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_TeamMembers_TeamId",
            //     table: "TeamMembers",
            //     column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AnnualLeaveBalance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BranchCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LeaveBalance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LeaveCycleStartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LinkedUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermitNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RateType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ShiftEndTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ShiftStartTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SickLeaveBalance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "CachedHourlyRate",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ClockInTime",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Users",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
