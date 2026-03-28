using BilheteriaAPI.Models;
using BilheteriaAPI.Repositories.Interfaces;

namespace BilheteriaAPI.Services;

public record CriarEventoRequest(
    string Nome,
    string Descricao,
    DateTime Data,
    string Local,
    string ImagemUrl,
    List<CriarSetorRequest> Setores);

public record CriarSetorRequest(string Nome, decimal Preco, int QuantidadeTotal);

public class EventoService(IEventoRepository eventoRepo)
{
    public async Task<IEnumerable<Evento>> ListarTodosAsync() =>
        await eventoRepo.GetAllAsync();

    public async Task<Evento?> ObterComSetoresAsync(int id) =>
        await eventoRepo.GetByIdWithSetoresAsync(id);

    public async Task<Evento> CriarAsync(CriarEventoRequest request)
    {
        var evento = new Evento
        {
            Nome = request.Nome,
            Descricao = request.Descricao,
            Data = request.Data,
            Local = request.Local,
            ImagemUrl = request.ImagemUrl
        };

        foreach (var s in request.Setores)
        {
            var setor = new Setor
            {
                Nome = s.Nome,
                Preco = s.Preco,
                QuantidadeTotal = s.QuantidadeTotal,
                QuantidadeDisponivel = s.QuantidadeTotal
            };
            for (int i = 1; i <= s.QuantidadeTotal; i++)
            {
                var numero = $"{(char)('A' + (i - 1) / 10)}{((i - 1) % 10) + 1}";
                setor.Assentos.Add(new Assento { Numero = numero, Status = StatusAssento.Disponivel });
            }
            evento.Setores.Add(setor);
        }

        return await eventoRepo.CreateAsync(evento);
    }

    public async Task<bool> ExcluirAsync(int id) =>
        await eventoRepo.DeleteAsync(id);
}
