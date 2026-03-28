using BilheteriaAPI.Models;

namespace BilheteriaAPI.Services;

public class PagamentoService
{
    public string RealizarCheckout(Pagamento metodoPagamento)
    {
        if (metodoPagamento.ValorTotal <= 0)
            throw new ArgumentException("O valor do pagamento deve ser maior que zero.");

        return metodoPagamento.Processar();
    }
}
