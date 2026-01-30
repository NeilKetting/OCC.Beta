using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HseqAuditAttachments_HseqAudits_AuditId",
                table: "HseqAuditAttachments");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_HseqAuditAttachments_HseqAudits_AuditId",
                table: "HseqAuditAttachments",
                column: "AuditId",
                principalTable: "HseqAudits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HseqAuditAttachments_HseqAudits_AuditId",
                table: "HseqAuditAttachments");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Projects");

            migrationBuilder.AddForeignKey(
                name: "FK_HseqAuditAttachments_HseqAudits_AuditId",
                table: "HseqAuditAttachments",
                column: "AuditId",
                principalTable: "HseqAudits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
