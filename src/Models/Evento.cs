namespace BilheteriaAPI.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Local { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = "🎉";
    public bool Destaque { get; set; } = false;
    public ICollection<Setor> Setores { get; set; } = [];
}
