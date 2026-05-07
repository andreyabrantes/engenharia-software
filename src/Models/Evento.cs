namespace BilheteriaAPI.Models;

/// <summary>
/// Representa um evento disponível para venda de ingressos.
/// Tabela: Eventos
/// PK: Id (INTEGER AUTOINCREMENT)
/// </summary>
public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int CapacidadeTotal { get; set; }
    public string DataEvento { get; set; } = string.Empty;
    public decimal PrecoPadrao { get; set; }
}
