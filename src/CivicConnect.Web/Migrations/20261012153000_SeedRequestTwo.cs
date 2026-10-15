using CivicConnect.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Migrations;

[DbContext(typeof(CivicConnectDbContext))]
[Migration("20261012153000_SeedRequestTwo")]
public partial class SeedRequestTwo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO "Requests" ("Id", "Title", "Description", "CategoryId", "State", "CreatedAt", "RequesterId", "AssigneeId")
            VALUES (2, 'Pothole on Main Street', 'A large pothole is affecting traffic near the community center.', 1, 0, '2026-10-01T08:00:00Z', 'requester-2', NULL);
            """);

        migrationBuilder.Sql("""
            INSERT INTO "Comments" ("Id", "RequestId", "Text", "CreatedAt", "CreatedBy", "IsInternal")
            VALUES (1, 2, 'Thank you for reporting this issue. Our team will investigate it.', '2026-10-01T09:00:00Z', 'staff-1', false);
            """);

        migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Requests\"', 'Id'), GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) + 1 FROM \"Requests\"), nextval(pg_get_serial_sequence('\"Requests\"', 'Id'))), false);");
        migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Comments\"', 'Id'), GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) + 1 FROM \"Comments\"), nextval(pg_get_serial_sequence('\"Comments\"', 'Id'))), false);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DELETE FROM "Comments" WHERE "Id" = 1;""");
        migrationBuilder.Sql("""DELETE FROM "Requests" WHERE "Id" = 2;""");
    }
}
