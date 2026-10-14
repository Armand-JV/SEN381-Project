using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CivicConnect.Web.Migrations
{
    /// <inheritdoc />
    public partial class M3P3_AuditLog_And_QueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Actor = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    BeforeSummary = table.Column<string>(type: "text", nullable: true),
                    AfterSummary = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AssigneeId",
                table: "Requests",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AssigneeId_State",
                table: "Requests",
                columns: new[] { "AssigneeId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequesterId",
                table: "Requests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_State",
                table: "Requests",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_State_CategoryId",
                table: "Requests",
                columns: new[] { "State", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Requests_AssigneeId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_AssigneeId_State",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CreatedAt",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_RequesterId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_State",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_State_CategoryId",
                table: "Requests");
        }
    }
}
