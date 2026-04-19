using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilheteriaAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "SenhaHash",
                value: "$2a$11$vp68k5RW6BWznVfj3PVwT.ayxWVVjVoerC9FODZQZMrVDwuylavJC");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "SenhaHash",
                value: "$2a$11$Je0gQ8uXDwFuxos4jfyRUe85JOLX8d84VqXss5fAe1ptz7cXmBcZ2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "SenhaHash",
                value: "$2a$11$Sj6FFqXRMKbqptoNe8/ESuTzs5q2ar5H2NrCBsw4TBfUka1eGXosm");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "SenhaHash",
                value: "$2a$11$pUX86BEuYqjBB.AU/12rnOv.u3R7YWB5.e8ndOI8g6noHjnO66Ruq");
        }
    }
}
