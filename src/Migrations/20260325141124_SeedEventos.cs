using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilheteriaAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("Eventos", new[] { "Id", "Nome", "Descricao", "Data", "Local", "ImagemUrl" }, new object[,]
            {
                { 1, "Show de Rock 2026", "O maior show de rock do ano com bandas internacionais!", new DateTime(2026, 8, 15, 20, 0, 0), "Arena Unifeso", "🎸" },
                { 2, "Festival de Música Eletrônica", "Uma noite inesquecível com os melhores DJs do mundo", new DateTime(2026, 9, 5, 22, 0, 0), "Clube Teresópolis", "🎧" },
                { 3, "Teatro: A Comédia dos Erros", "Peça clássica de Shakespeare com elenco renomado", new DateTime(2026, 7, 20, 19, 0, 0), "Teatro Municipal", "🎭" }
            });

            migrationBuilder.InsertData("Setores", new[] { "Id", "Nome", "Preco", "QuantidadeTotal", "QuantidadeDisponivel", "EventoId" }, new object[,]
            {
                { 1, "Pista",     80.00m, 50, 50, 1 },
                { 2, "Camarote", 200.00m, 20, 20, 1 },
                { 3, "Pista",    100.00m, 60, 60, 2 },
                { 4, "VIP",      250.00m, 15, 15, 2 },
                { 5, "Plateia",   60.00m, 40, 40, 3 },
                { 6, "Balcão",    40.00m, 20, 20, 3 }
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("Setores", "EventoId", new object[] { 1, 2, 3 });
            migrationBuilder.DeleteData("Eventos", "Id", new object[] { 1, 2, 3 });
        }
    }
}
