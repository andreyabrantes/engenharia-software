using System.Net.Http.Json;
using BilheteriaVirtualBlazor.Models;

namespace BilheteriaVirtualBlazor.Services;

public class EventoService
{
    private readonly HttpClient _http;

    public event Action? OnEstadoAlterado;

    public EventoService(HttpClient http)
    {
        _http = http;
    }

    // -------------------------------------------------------------------------
    // EVENTOS
    // -------------------------------------------------------------------------

    public async Task<List<Evento>> ObterEventosAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<Evento>>("api/eventos")
                   ?? new List<Evento>();
        }
        catch
        {
            return new List<Evento>();
        }
    }

    /// <summary>
    /// Versão síncrona mantida para compatibilidade com as páginas existentes.
    /// Executa a chamada HTTP de forma bloqueante — aceitável em WASM single-thread.
    /// </summary>
    public List<Evento> ObterEventos()
        => ObterEventosAsync().GetAwaiter().GetResult();

    public async Task<Evento?> ObterEventoPorIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<Evento>($"api/eventos/{id}");
        }
        catch
        {
            return null;
        }
    }

    public Evento? ObterEventoPorId(int id)
        => ObterEventoPorIdAsync(id).GetAwaiter().GetResult();

    // -------------------------------------------------------------------------
    // ASSENTOS
    // -------------------------------------------------------------------------

    public async Task<List<Assento>> ObterAssentosAsync(int eventoId, int setorId)
    {
        try
        {
            var assentos = await _http.GetFromJsonAsync<List<Assento>>(
                $"api/eventos/{eventoId}/setores/{setorId}/assentos")
                ?? new List<Assento>();

            // Garante que Selecionado começa sempre a false (campo local, não existe na API)
            foreach (var a in assentos)
                a.Selecionado = false;

            return assentos;
        }
        catch
        {
            return new List<Assento>();
        }
    }

    public List<Assento> ObterAssentos(int eventoId, int setorId)
        => ObterAssentosAsync(eventoId, setorId).GetAwaiter().GetResult();

    public async Task<string?> UploadImagemAsync(string base64, string nomeArquivo)
    {
        try
        {
            var ext = Path.GetExtension(nomeArquivo).ToLower();
            var response = await _http.PostAsJsonAsync("api/eventos/upload-imagem",
                new { Base64 = base64, Extensao = ext });
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<UploadResult>();
            return result?.Caminho;
        }
        catch { return null; }
    }

    private sealed class UploadResult { public string Caminho { get; set; } = string.Empty; }

    // -------------------------------------------------------------------------
    // CRIAR EVENTO (admin)
    // -------------------------------------------------------------------------

    public async Task<(bool sucesso, string erro)> AdicionarEventoAsync(Evento evento)
    {
        var (valido, erroValidacao) = ValidarEvento(evento);
        if (!valido) return (false, erroValidacao);

        try
        {
            var request = new CriarEventoRequest(
                evento.Nome,
                evento.Descricao,
                evento.Data,
                evento.Local,
                evento.ImagemUrl,
                evento.Setores.Select(s => new CriarSetorRequest(
                    s.Nome, s.Preco, s.QuantidadeTotal)).ToList()
            );

            var response = await _http.PostAsJsonAsync("api/eventos", request);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                return (false, $"Erro ao cadastrar evento: {erro}");
            }

            OnEstadoAlterado?.Invoke();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"Erro de comunicação: {ex.Message}");
        }
    }

    public (bool sucesso, string erro) AdicionarEvento(Evento evento)
        => AdicionarEventoAsync(evento).GetAwaiter().GetResult();

    // -------------------------------------------------------------------------
    // EXCLUIR EVENTO (admin)
    // -------------------------------------------------------------------------

    public async Task<bool> ExcluirEventoAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/eventos/{id}");
            if (!response.IsSuccessStatusCode) return false;
            OnEstadoAlterado?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ExcluirEvento(int id)
        => ExcluirEventoAsync(id).GetAwaiter().GetResult();

    // -------------------------------------------------------------------------
    // COMPRAR INGRESSO
    // -------------------------------------------------------------------------

    public async Task<(bool sucesso, string mensagem, string codigoIngresso)> FinalizarCompraAsync(
        string nomeCliente, string email, int eventoId, int setorId, List<int> assentoIds)
    {
        try
        {
            var request = new ComprarIngressoRequest(
                nomeCliente, email, eventoId, setorId, assentoIds);

            var response = await _http.PostAsJsonAsync("api/ingressos", request);

            if (!response.IsSuccessStatusCode)
            {
                // A API devolve { mensagem } em caso de conflito (assento ocupado)
                var erro = await response.Content.ReadFromJsonAsync<MensagemErro>();
                return (false, erro?.Mensagem ?? "Erro ao processar compra.", string.Empty);
            }

            var resultado = await response.Content.ReadFromJsonAsync<CompraResultado>();
            if (resultado is null)
                return (false, "Resposta inválida da API.", string.Empty);

            OnEstadoAlterado?.Invoke();
            return (true, resultado.Mensagem, resultado.CodigoIngresso);
        }
        catch (Exception ex)
        {
            return (false, $"Erro de comunicação: {ex.Message}", string.Empty);
        }
    }

    public (bool sucesso, string mensagem, string codigoIngresso) FinalizarCompra(
        string nomeCliente, string email, int eventoId, int setorId, List<int> assentoIds)
        => FinalizarCompraAsync(nomeCliente, email, eventoId, setorId, assentoIds)
               .GetAwaiter().GetResult();

    // -------------------------------------------------------------------------
    // MEUS INGRESSOS
    // -------------------------------------------------------------------------

    public async Task<List<Compra>> ObterComprasPorEmailAsync(string email)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<Compra>>(
                $"api/ingressos/email/{Uri.EscapeDataString(email)}")
                ?? new List<Compra>();
        }
        catch
        {
            return new List<Compra>();
        }
    }

    public List<Compra> ObterComprasPorEmail(string email)
        => ObterComprasPorEmailAsync(email).GetAwaiter().GetResult();

    // -------------------------------------------------------------------------
    // RELATÓRIOS (admin)
    // -------------------------------------------------------------------------

    public async Task<RelatorioVendas> ObterRelatorioAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<RelatorioVendas>("api/relatorios")
                   ?? new RelatorioVendas();
        }
        catch
        {
            return new RelatorioVendas();
        }
    }

    public RelatorioVendas ObterRelatorio()
        => ObterRelatorioAsync().GetAwaiter().GetResult();

    // -------------------------------------------------------------------------
    // VALIDAÇÃO LOCAL (mantida para feedback imediato no formulário)
    // -------------------------------------------------------------------------

    public (bool sucesso, string erro) ValidarEvento(Evento evento)
    {
        if (string.IsNullOrWhiteSpace(evento.Nome))
            return (false, "Nome do evento é obrigatório.");
        if (string.IsNullOrWhiteSpace(evento.Local))
            return (false, "Local do evento é obrigatório.");
        if (string.IsNullOrWhiteSpace(evento.Descricao))
            return (false, "Descrição do evento é obrigatória.");
        if (evento.Data <= DateTime.Now)
            return (false, "A data do evento deve ser no futuro.");
        if (!evento.Setores.Any())
            return (false, "Adicione pelo menos um setor.");

        foreach (var s in evento.Setores)
        {
            if (string.IsNullOrWhiteSpace(s.Nome))
                return (false, "Todos os setores precisam de um nome.");
            if (s.Preco <= 0)
                return (false, $"O preço do setor '{s.Nome}' deve ser maior que zero.");
            if (s.QuantidadeTotal <= 0)
                return (false, $"A quantidade do setor '{s.Nome}' deve ser maior que zero.");
        }

        return (true, string.Empty);
    }

    // -------------------------------------------------------------------------
    // DTOs internos (espelham os records da API)
    // -------------------------------------------------------------------------

    private sealed record CriarEventoRequest(
        string Nome,
        string Descricao,
        DateTime Data,
        string Local,
        string ImagemUrl,
        List<CriarSetorRequest> Setores);

    private sealed record CriarSetorRequest(
        string Nome,
        decimal Preco,
        int QuantidadeTotal);

    private sealed record ComprarIngressoRequest(
        string NomeCliente,
        string Email,
        int EventoId,
        int SetorId,
        List<int> AssentoIds);

    private sealed class CompraResultado
    {
        public bool   Sucesso         { get; set; }
        public string Mensagem        { get; set; } = string.Empty;
        public string CodigoIngresso  { get; set; } = string.Empty;
    }

    private sealed class MensagemErro
    {
        public string Mensagem { get; set; } = string.Empty;
    }
}
