using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BilheteriaAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Local = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    QuantidadeTotal = table.Column<int>(type: "int", nullable: false),
                    QuantidadeDisponivel = table.Column<int>(type: "int", nullable: false),
                    EventoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Setores_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SetorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assentos_Setores_SetorId",
                        column: x => x.SetorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingressos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoUnico = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    AssentoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingressos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingressos_Assentos_AssentoId",
                        column: x => x.AssentoId,
                        principalTable: "Assentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ingressos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Eventos",
                columns: new[] { "Id", "Data", "Descricao", "ImagemUrl", "Local", "Nome" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 15, 20, 0, 0, 0, DateTimeKind.Unspecified), "O maior show de rock do ano com bandas internacionais!", "🎸", "Arena Unifeso", "Show de Rock 2024" },
                    { 2, new DateTime(2025, 9, 5, 22, 0, 0, 0, DateTimeKind.Unspecified), "Uma noite inesquecível com os melhores DJs do mundo", "🎧", "Clube Teresópolis", "Festival de Música Eletrônica" },
                    { 3, new DateTime(2025, 7, 20, 19, 0, 0, 0, DateTimeKind.Unspecified), "Peça clássica de Shakespeare com elenco renomado", "🎭", "Teatro Municipal", "Teatro: A Comédia dos Erros" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "Nome", "SenhaHash", "Tipo" },
                values: new object[,]
                {
                    { 1, "admin@bilheteria.com", "Administrador", "$2a$11$Sj6FFqXRMKbqptoNe8/ESuTzs5q2ar5H2NrCBsw4TBfUka1eGXosm", "Admin" },
                    { 2, "cliente@email.com", "Cliente Teste", "$2a$11$pUX86BEuYqjBB.AU/12rnOv.u3R7YWB5.e8ndOI8g6noHjnO66Ruq", "Cliente" }
                });

            migrationBuilder.InsertData(
                table: "Setores",
                columns: new[] { "Id", "EventoId", "Nome", "Preco", "QuantidadeDisponivel", "QuantidadeTotal" },
                values: new object[,]
                {
                    { 1, 1, "Pista", 80.00m, 50, 50 },
                    { 2, 1, "Camarote", 200.00m, 20, 20 },
                    { 3, 2, "Pista", 100.00m, 60, 60 },
                    { 4, 2, "VIP", 250.00m, 15, 15 },
                    { 5, 3, "Plateia", 60.00m, 40, 40 },
                    { 6, 3, "Balcão", 40.00m, 20, 20 }
                });

            migrationBuilder.InsertData(
                table: "Assentos",
                columns: new[] { "Id", "Numero", "SetorId", "Status" },
                values: new object[,]
                {
                    { 1, "A1", 1, 0 },
                    { 2, "A2", 1, 0 },
                    { 3, "A3", 1, 0 },
                    { 4, "A4", 1, 0 },
                    { 5, "A5", 1, 0 },
                    { 6, "A6", 1, 0 },
                    { 7, "A7", 1, 0 },
                    { 8, "A8", 1, 0 },
                    { 9, "A9", 1, 0 },
                    { 10, "A10", 1, 0 },
                    { 11, "B1", 1, 0 },
                    { 12, "B2", 1, 0 },
                    { 13, "B3", 1, 0 },
                    { 14, "B4", 1, 0 },
                    { 15, "B5", 1, 0 },
                    { 16, "B6", 1, 0 },
                    { 17, "B7", 1, 0 },
                    { 18, "B8", 1, 0 },
                    { 19, "B9", 1, 0 },
                    { 20, "B10", 1, 0 },
                    { 21, "C1", 1, 0 },
                    { 22, "C2", 1, 0 },
                    { 23, "C3", 1, 0 },
                    { 24, "C4", 1, 0 },
                    { 25, "C5", 1, 0 },
                    { 26, "C6", 1, 0 },
                    { 27, "C7", 1, 0 },
                    { 28, "C8", 1, 0 },
                    { 29, "C9", 1, 0 },
                    { 30, "C10", 1, 0 },
                    { 31, "D1", 1, 0 },
                    { 32, "D2", 1, 0 },
                    { 33, "D3", 1, 0 },
                    { 34, "D4", 1, 0 },
                    { 35, "D5", 1, 0 },
                    { 36, "D6", 1, 0 },
                    { 37, "D7", 1, 0 },
                    { 38, "D8", 1, 0 },
                    { 39, "D9", 1, 0 },
                    { 40, "D10", 1, 0 },
                    { 41, "E1", 1, 0 },
                    { 42, "E2", 1, 0 },
                    { 43, "E3", 1, 0 },
                    { 44, "E4", 1, 0 },
                    { 45, "E5", 1, 0 },
                    { 46, "E6", 1, 0 },
                    { 47, "E7", 1, 0 },
                    { 48, "E8", 1, 0 },
                    { 49, "E9", 1, 0 },
                    { 50, "E10", 1, 0 },
                    { 51, "A1", 2, 0 },
                    { 52, "A2", 2, 0 },
                    { 53, "A3", 2, 0 },
                    { 54, "A4", 2, 0 },
                    { 55, "A5", 2, 0 },
                    { 56, "A6", 2, 0 },
                    { 57, "A7", 2, 0 },
                    { 58, "A8", 2, 0 },
                    { 59, "A9", 2, 0 },
                    { 60, "A10", 2, 0 },
                    { 61, "B1", 2, 0 },
                    { 62, "B2", 2, 0 },
                    { 63, "B3", 2, 0 },
                    { 64, "B4", 2, 0 },
                    { 65, "B5", 2, 0 },
                    { 66, "B6", 2, 0 },
                    { 67, "B7", 2, 0 },
                    { 68, "B8", 2, 0 },
                    { 69, "B9", 2, 0 },
                    { 70, "B10", 2, 0 },
                    { 71, "A1", 3, 0 },
                    { 72, "A2", 3, 0 },
                    { 73, "A3", 3, 0 },
                    { 74, "A4", 3, 0 },
                    { 75, "A5", 3, 0 },
                    { 76, "A6", 3, 0 },
                    { 77, "A7", 3, 0 },
                    { 78, "A8", 3, 0 },
                    { 79, "A9", 3, 0 },
                    { 80, "A10", 3, 0 },
                    { 81, "B1", 3, 0 },
                    { 82, "B2", 3, 0 },
                    { 83, "B3", 3, 0 },
                    { 84, "B4", 3, 0 },
                    { 85, "B5", 3, 0 },
                    { 86, "B6", 3, 0 },
                    { 87, "B7", 3, 0 },
                    { 88, "B8", 3, 0 },
                    { 89, "B9", 3, 0 },
                    { 90, "B10", 3, 0 },
                    { 91, "C1", 3, 0 },
                    { 92, "C2", 3, 0 },
                    { 93, "C3", 3, 0 },
                    { 94, "C4", 3, 0 },
                    { 95, "C5", 3, 0 },
                    { 96, "C6", 3, 0 },
                    { 97, "C7", 3, 0 },
                    { 98, "C8", 3, 0 },
                    { 99, "C9", 3, 0 },
                    { 100, "C10", 3, 0 },
                    { 101, "D1", 3, 0 },
                    { 102, "D2", 3, 0 },
                    { 103, "D3", 3, 0 },
                    { 104, "D4", 3, 0 },
                    { 105, "D5", 3, 0 },
                    { 106, "D6", 3, 0 },
                    { 107, "D7", 3, 0 },
                    { 108, "D8", 3, 0 },
                    { 109, "D9", 3, 0 },
                    { 110, "D10", 3, 0 },
                    { 111, "E1", 3, 0 },
                    { 112, "E2", 3, 0 },
                    { 113, "E3", 3, 0 },
                    { 114, "E4", 3, 0 },
                    { 115, "E5", 3, 0 },
                    { 116, "E6", 3, 0 },
                    { 117, "E7", 3, 0 },
                    { 118, "E8", 3, 0 },
                    { 119, "E9", 3, 0 },
                    { 120, "E10", 3, 0 },
                    { 121, "F1", 3, 0 },
                    { 122, "F2", 3, 0 },
                    { 123, "F3", 3, 0 },
                    { 124, "F4", 3, 0 },
                    { 125, "F5", 3, 0 },
                    { 126, "F6", 3, 0 },
                    { 127, "F7", 3, 0 },
                    { 128, "F8", 3, 0 },
                    { 129, "F9", 3, 0 },
                    { 130, "F10", 3, 0 },
                    { 131, "A1", 4, 0 },
                    { 132, "A2", 4, 0 },
                    { 133, "A3", 4, 0 },
                    { 134, "A4", 4, 0 },
                    { 135, "A5", 4, 0 },
                    { 136, "A6", 4, 0 },
                    { 137, "A7", 4, 0 },
                    { 138, "A8", 4, 0 },
                    { 139, "A9", 4, 0 },
                    { 140, "A10", 4, 0 },
                    { 141, "B1", 4, 0 },
                    { 142, "B2", 4, 0 },
                    { 143, "B3", 4, 0 },
                    { 144, "B4", 4, 0 },
                    { 145, "B5", 4, 0 },
                    { 146, "A1", 5, 0 },
                    { 147, "A2", 5, 0 },
                    { 148, "A3", 5, 0 },
                    { 149, "A4", 5, 0 },
                    { 150, "A5", 5, 0 },
                    { 151, "A6", 5, 0 },
                    { 152, "A7", 5, 0 },
                    { 153, "A8", 5, 0 },
                    { 154, "A9", 5, 0 },
                    { 155, "A10", 5, 0 },
                    { 156, "B1", 5, 0 },
                    { 157, "B2", 5, 0 },
                    { 158, "B3", 5, 0 },
                    { 159, "B4", 5, 0 },
                    { 160, "B5", 5, 0 },
                    { 161, "B6", 5, 0 },
                    { 162, "B7", 5, 0 },
                    { 163, "B8", 5, 0 },
                    { 164, "B9", 5, 0 },
                    { 165, "B10", 5, 0 },
                    { 166, "C1", 5, 0 },
                    { 167, "C2", 5, 0 },
                    { 168, "C3", 5, 0 },
                    { 169, "C4", 5, 0 },
                    { 170, "C5", 5, 0 },
                    { 171, "C6", 5, 0 },
                    { 172, "C7", 5, 0 },
                    { 173, "C8", 5, 0 },
                    { 174, "C9", 5, 0 },
                    { 175, "C10", 5, 0 },
                    { 176, "D1", 5, 0 },
                    { 177, "D2", 5, 0 },
                    { 178, "D3", 5, 0 },
                    { 179, "D4", 5, 0 },
                    { 180, "D5", 5, 0 },
                    { 181, "D6", 5, 0 },
                    { 182, "D7", 5, 0 },
                    { 183, "D8", 5, 0 },
                    { 184, "D9", 5, 0 },
                    { 185, "D10", 5, 0 },
                    { 186, "A1", 6, 0 },
                    { 187, "A2", 6, 0 },
                    { 188, "A3", 6, 0 },
                    { 189, "A4", 6, 0 },
                    { 190, "A5", 6, 0 },
                    { 191, "A6", 6, 0 },
                    { 192, "A7", 6, 0 },
                    { 193, "A8", 6, 0 },
                    { 194, "A9", 6, 0 },
                    { 195, "A10", 6, 0 },
                    { 196, "B1", 6, 0 },
                    { 197, "B2", 6, 0 },
                    { 198, "B3", 6, 0 },
                    { 199, "B4", 6, 0 },
                    { 200, "B5", 6, 0 },
                    { 201, "B6", 6, 0 },
                    { 202, "B7", 6, 0 },
                    { 203, "B8", 6, 0 },
                    { 204, "B9", 6, 0 },
                    { 205, "B10", 6, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assentos_SetorId",
                table: "Assentos",
                column: "SetorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_AssentoId",
                table: "Ingressos",
                column: "AssentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_CodigoUnico",
                table: "Ingressos",
                column: "CodigoUnico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_UsuarioId",
                table: "Ingressos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Setores_EventoId",
                table: "Setores",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingressos");

            migrationBuilder.DropTable(
                name: "Assentos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Setores");

            migrationBuilder.DropTable(
                name: "Eventos");
        }
    }
}
