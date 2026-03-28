namespace BilheteriaAPI.Models;

public class Setor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeTotal { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public int EventoId { get; set; }
    public Evento Evento { get; set; } = null!;
    public ICollection<Assento> Assentos { get; set; } = [];
}
