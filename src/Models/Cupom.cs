namespace BilheteriaAPI.Models;

/// <summary>
/// Representa um cupom de desconto.
/// Tabela: Cupons
/// PK: Codigo (VARCHAR(50))
/// </summary>
public class Cupom
{
    public string Codigo { get; set; } = string.Empty;
    public decimal PorcentagemDesconto { get; set; }
    public decimal ValorMinimoRegra { get; set; }
}
