using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCC.Client.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely add missing columns via raw SQL check
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'TaxNumber') IS NULL BEGIN ALTER TABLE Employees ADD TaxNumber nvarchar(max) NOT NULL DEFAULT N'' END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'BankName') IS NULL BEGIN ALTER TABLE Employees ADD BankName nvarchar(max) NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'AccountNumber') IS NULL BEGIN ALTER TABLE Employees ADD AccountNumber nvarchar(max) NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'BranchCode') IS NULL BEGIN ALTER TABLE Employees ADD BranchCode nvarchar(max) NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'AccountType') IS NULL BEGIN ALTER TABLE Employees ADD AccountType nvarchar(max) NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'RateType') IS NULL BEGIN ALTER TABLE Employees ADD RateType int NOT NULL DEFAULT 0 END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'LeaveBalance') IS NULL BEGIN ALTER TABLE Employees ADD LeaveBalance float NOT NULL DEFAULT 0.0 END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'AnnualLeaveBalance') IS NULL BEGIN ALTER TABLE Employees ADD AnnualLeaveBalance float NOT NULL DEFAULT 0.0 END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'SickLeaveBalance') IS NULL BEGIN ALTER TABLE Employees ADD SickLeaveBalance float NOT NULL DEFAULT 0.0 END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'LeaveCycleStartDate') IS NULL BEGIN ALTER TABLE Employees ADD LeaveCycleStartDate datetime2 NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'LinkedUserId') IS NULL BEGIN ALTER TABLE Employees ADD LinkedUserId uniqueidentifier NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'ShiftStartTime') IS NULL BEGIN ALTER TABLE Employees ADD ShiftStartTime time NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'ShiftEndTime') IS NULL BEGIN ALTER TABLE Employees ADD ShiftEndTime time NULL END");
            migrationBuilder.Sql("IF COL_LENGTH('Employees', 'Status') IS NULL BEGIN ALTER TABLE Employees ADD Status int NOT NULL DEFAULT 0 END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
