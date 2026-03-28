using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilheteriaAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixEventos2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Data", "Nome" },
                values: new object[] { new DateTime(2026, 8, 15, 20, 0, 0, 0, DateTimeKind.Unspecified), "Show de Rock 2026" });

            migrationBuilder.UpdateData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Data",
                value: new DateTime(2026, 9, 5, 22, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Data",
                value: new DateTime(2026, 7, 20, 19, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Data", "Nome" },
                values: new object[] { new DateTime(2025, 8, 15, 20, 0, 0, 0, DateTimeKind.Unspecified), "Show de Rock 2024" });

            migrationBuilder.UpdateData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Data",
                value: new DateTime(2025, 9, 5, 22, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Data",
                value: new DateTime(2025, 7, 20, 19, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
