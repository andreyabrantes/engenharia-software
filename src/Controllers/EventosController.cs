using BilheteriaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilheteriaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventosController(EventoService eventoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await eventoService.ListarTodosAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var evento = await eventoService.ObterComSetoresAsync(id);
        if (evento is null) return NotFound(new { mensagem = "Evento não encontrado." });
        return Ok(evento);
    }

    [HttpGet("{eventoId}/setores/{setorId}/assentos")]
    public async Task<IActionResult> ObterAssentos(int eventoId, int setorId)
    {
        var evento = await eventoService.ObterComSetoresAsync(eventoId);
        if (evento is null) return NotFound(new { mensagem = "Evento não encontrado." });
        var setor = evento.Setores.FirstOrDefault(s => s.Id == setorId);
        if (setor is null) return NotFound(new { mensagem = "Setor não encontrado." });
        return Ok(setor.Assentos);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarEventoRequest request)
    {
        try
        {
            var evento = await eventoService.CriarAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = evento.Id }, evento);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro interno.", detalhe = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var removido = await eventoService.ExcluirAsync(id);
        if (!removido) return NotFound(new { mensagem = "Evento não encontrado." });
        return Ok(new { mensagem = "Evento excluído com sucesso." });
    }
}
