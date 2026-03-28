using BilheteriaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilheteriaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngressosController(IngressoService ingressoService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Comprar([FromBody] ComprarIngressoRequest request)
    {
        try
        {
            var resultado = await ingressoService.ComprarAsync(request);
            if (!resultado.Sucesso) return Conflict(new { mensagem = resultado.Mensagem });
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro interno.", detalhe = ex.Message });
        }
    }

    [HttpGet("usuario/{usuarioId}")]
    public async Task<IActionResult> ListarPorUsuario(int usuarioId) =>
        Ok(await ingressoService.ListarPorUsuarioAsync(usuarioId));

    [HttpGet("email/{email}")]
    public async Task<IActionResult> ListarPorEmail(string email) =>
        Ok(await ingressoService.ListarPorEmailAsync(email));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var (sucesso, mensagem) = await ingressoService.CancelarAsync(id);
        if (!sucesso) return NotFound(new { mensagem });
        return Ok(new { mensagem });
    }
}
