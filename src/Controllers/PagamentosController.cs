using Microsoft.AspNetCore.Mvc;
using BilheteriaAPI.Models;
using BilheteriaAPI.Services;

namespace BilheteriaAPI.Controllers; // Usando namespace moderno do .NET 8 (sem chaves)

[Route("api/[controller]")]
[ApiController]
public class PagamentosController : ControllerBase
{
    private readonly PagamentoService _pagamentoService;

    public PagamentosController()
    {
        _pagamentoService = new PagamentoService();
    }

    [HttpPost("checkout")]
    public IActionResult Checkout([FromBody] CheckoutRequest request)
    {
        Pagamento pagamento;

        // Factory: Decide qual classe instanciar baseado na string do front-end
        if (request.TipoPagamento.ToUpper() == "PIX")
        {
            pagamento = new PagamentoPix
            {
                ValorTotal = request.Valor,
                ChavePixOrigem = request.DadosPagamento
            };
        }
        else if (request.TipoPagamento.ToUpper() == "CARTAO")
        {
            pagamento = new PagamentoCartao
            {
                ValorTotal = request.Valor,
                NumeroCartao = request.DadosPagamento,
                Titular = "Cliente C# POO"
            };
        }
        else
        {
            return BadRequest(new { Mensagem = "Tipo de pagamento inválido." });
        }

        // Chama o serviço usando Polimorfismo
        var resultado = _pagamentoService.RealizarCheckout(pagamento);

        return Ok(new 
        { 
            Status = pagamento.Status, 
            Mensagem = resultado,
            DataHora = pagamento.DataPagamento
        });
    }
}

// Classe DTO limpa no final do arquivo
public class CheckoutRequest
{
    public decimal Valor { get; set; }
    public string TipoPagamento { get; set; } = string.Empty;
    public string DadosPagamento { get; set; } = string.Empty;
}