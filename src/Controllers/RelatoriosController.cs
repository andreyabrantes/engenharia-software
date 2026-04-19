using BilheteriaAPI.Data;
using BilheteriaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilheteriaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterRelatorio()
    {
        var eventos = await db.Eventos
            .Include(e => e.Setores).ThenInclude(s => s.Assentos)
            .ToListAsync();

        var ingressos = await db.Ingressos
            .Include(i => i.Assento).ThenInclude(a => a.Setor).ThenInclude(s => s.Evento)
            .Where(i => i.Status == StatusIngresso.Ativo)
            .ToListAsync();

        var vendasPorEvento = ingressos
            .GroupBy(i => i.Assento.Setor.EventoId)
            .Select(g =>
            {
                var evento = eventos.FirstOrDefault(e => e.Id == g.Key);
                var receita = g.Sum(i => i.Assento.Setor.Preco);
                var capacidade = evento?.Setores.Sum(s => s.QuantidadeTotal) ?? 0;
                return new
                {
                    NomeEvento = evento?.Nome ?? "",
                    IngressosVendidos = g.Count(),
                    Receita = receita,
                    CapacidadeTotal = capacidade
                };
            }).ToList();

        var totalVendidos = ingressos.Count;
        var receitaTotal = ingressos.Sum(i => i.Assento.Setor.Preco);

        return Ok(new
        {
            TotalIngressosVendidos = totalVendidos,
            ReceitaTotal = receitaTotal,
            EventosAtivos = eventos.Count,
            TicketMedio = totalVendidos > 0 ? receitaTotal / totalVendidos : 0,
            VendasPorEvento = vendasPorEvento
        });
    }
}
