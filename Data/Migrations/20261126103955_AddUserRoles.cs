using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VeterinerKlinigi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "clinic-admin", "clinic-admin", "Admin", "ADMIN" },
                    { "clinic-member", "clinic-member", "Üye", "ÜYE" },
                    { "clinic-veterinarian", "clinic-veterinarian", "Veteriner", "VETERINER" }
                });

            // Identity ile daha önce açılan hesaplardaki üyelikleri koruyorum.
            migrationBuilder.Sql("""
                INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
                SELECT "Id", 'clinic-member'
                FROM "AspNetUsers" AS "User"
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetUserRoles" AS "Membership"
                    WHERE "Membership"."UserId" = "User"."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "clinic-admin");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "clinic-member");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "clinic-veterinarian");
        }
    }
}
