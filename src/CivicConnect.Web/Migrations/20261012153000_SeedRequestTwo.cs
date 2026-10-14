using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Migrations;

public partial class SeedRequestTwo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Requests",
            columns: new[]
            {
                "Id",
                "Title",
                "Description",
                "CategoryId",
                "State",
                "CreatedAt",
                "RequesterId",
                "AssigneeId"
            },
            values: new object[]
            {
                2,
                "Pothole on Main Street",
                "A large pothole is affecting traffic near the community center.",
                1,
                0,
                new System.DateTime(2026, 10, 1, 8, 0, 0, System.DateTimeKind.Utc),
                "requester-2",
                null
            });

        migrationBuilder.InsertData(
            table: "Comments",
            columns: new[] { "Id", "RequestId", "Text", "CreatedAt", "CreatedBy", "IsInternal" },
            values: new object[]
            {
                1,
                2,
                "Thank you for reporting this issue. Our team will investigate it.",
                new System.DateTime(2026, 10, 1, 9, 0, 0, System.DateTimeKind.Utc),
                "staff-1",
                false
            });

        migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Requests\"', 'Id'), GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) + 1 FROM \"Requests\"), nextval(pg_get_serial_sequence('\"Requests\"', 'Id'))), false);");
        migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Comments\"', 'Id'), GREATEST((SELECT COALESCE(MAX(\"Id\"), 0) + 1 FROM \"Comments\"), nextval(pg_get_serial_sequence('\"Comments\"', 'Id'))), false);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Comments",
            keyColumn: "Id",
            keyValue: 1);

        migrationBuilder.DeleteData(
            table: "Requests",
            keyColumn: "Id",
            keyValue: 2);
    }
}
