namespace BilheteriaAPI.Models;

public abstract class Pagamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal ValorTotal { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pendente";

    public abstract string Processar();
}

public class PagamentoPix : Pagamento
{
    public string ChavePixOrigem { get; set; } = string.Empty;

    public override string Processar()
    {
        Status = "Aprovado";
        return $"Pagamento via PIX aprovado. Chave de origem: {ChavePixOrigem}. " +
               $"Valor: R${ValorTotal:F2}. Transação: {Id}";
    }
}

public class PagamentoCartao : Pagamento
{
    public string NumeroCartao { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;

    public override string Processar()
    {
        if (NumeroCartao.Length < 16)
        {
            Status = "Recusado";
            return $"Pagamento via Cartão recusado. Número do cartão inválido (mínimo 16 dígitos). Titular: {Titular}.";
        }

        Status = "Aprovado";
        var finalCartao = NumeroCartao[^4..];
        return $"Pagamento via Cartão aprovado. Titular: {Titular}. " +
               $"Final do cartão: {finalCartao}. Valor: R${ValorTotal:F2}. Transação: {Id}";
    }
}

public class PagamentoBoleto : Pagamento
{
    public string CodigoBarras { get; set; } = Guid.NewGuid().ToString("N")[..20].ToUpper();

    public override string Processar()
    {
        Status = "Aprovado";
        return $"Pagamento via Boleto aprovado. Código de barras: {CodigoBarras}. " +
               $"Valor: R${ValorTotal:F2}. Transação: {Id}";
    }
}

public class PagamentoDinheiro : Pagamento
{
    public decimal ValorEntregue { get; set; }

    public override string Processar()
    {
        if (ValorEntregue < ValorTotal)
        {
            Status = "Recusado";
            return $"Pagamento em Dinheiro recusado. Valor entregue (R${ValorEntregue:F2}) insuficiente.";
        }

        Status = "Aprovado";
        var troco = ValorEntregue - ValorTotal;
        return $"Pagamento em Dinheiro aprovado. Valor: R${ValorTotal:F2}. " +
               $"Troco: R${troco:F2}. Transação: {Id}";
    }
}
