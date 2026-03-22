using BilheteriaVirtualBlazor.Models;

namespace BilheteriaVirtualBlazor.Services;

public class EmailService
{
    private readonly List<EmailEnviado> _historico = new();

    public event Action? OnEmailEnviado;

    public async Task<bool> EnviarIngressoAsync(Compra compra)
    {
        // Simula latência de envio SMTP
        await Task.Delay(800);

        var email = new EmailEnviado
        {
            Id = _historico.Count + 1,
            Destinatario = compra.Email,
            Assunto = $"🎫 Seu ingresso para {compra.EventoNome} chegou!",
            CorpoHtml = GerarCorpoEmail(compra),
            EnviadoEm = DateTime.Now,
            Sucesso = true
        };

        _historico.Add(email);
        OnEmailEnviado?.Invoke();

        // Em produção real: integrar com SendGrid, AWS SES, SMTP, etc.
        Console.WriteLine($"[EMAIL SIMULADO] Para: {email.Destinatario} | Assunto: {email.Assunto}");

        return true;
    }

    public List<EmailEnviado> ObterHistorico() => _historico.ToList();

    private static string GerarCorpoEmail(Compra compra)
    {
        var assentos = string.Join(", ", compra.NumerosAssentos);
        var dataFormatada = compra.EventoData.ToString("dddd, dd 'de' MMMM 'de' yyyy 'às' HH:mm",
            new System.Globalization.CultureInfo("pt-BR"));

        var template = """
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
              <meta charset="UTF-8"/>
              <style>
                body { font-family: 'Segoe UI', Arial, sans-serif; background: #f0f4ff; margin: 0; padding: 20px; }
                .wrapper { max-width: 600px; margin: 0 auto; }
                .header { background: linear-gradient(135deg, #6366f1, #ec4899); border-radius: 16px 16px 0 0; padding: 40px; text-align: center; color: white; }
                .header h1 { margin: 0; font-size: 2rem; }
                .header p { margin: 8px 0 0; opacity: 0.9; }
                .body { background: white; padding: 32px; }
                .ticket { border: 2px dashed #6366f1; border-radius: 12px; overflow: hidden; margin: 24px 0; }
                .ticket-header { background: linear-gradient(135deg, #6366f1, #ec4899); color: white; padding: 20px 24px; display: flex; align-items: center; gap: 16px; }
                .ticket-emoji { font-size: 2.5rem; }
                .ticket-title { font-size: 1.3rem; font-weight: 700; margin: 0; }
                .ticket-subtitle { margin: 4px 0 0; opacity: 0.9; font-size: 0.9rem; }
                .ticket-body { padding: 24px; }
                .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 20px; }
                .info-item label { display: block; font-size: 0.75rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; font-weight: 600; margin-bottom: 4px; }
                .info-item span { font-weight: 600; color: #1e293b; }
                .codigo-box { background: #f8fafc; border-radius: 8px; padding: 20px; text-align: center; border: 1px solid #e2e8f0; }
                .codigo-label { font-size: 0.8rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; font-weight: 600; }
                .codigo { font-size: 1.8rem; font-weight: 700; color: #6366f1; font-family: monospace; letter-spacing: 0.1em; margin: 8px 0 4px; }
                .codigo-hint { font-size: 0.85rem; color: #64748b; }
                .total-box { background: linear-gradient(135deg, #6366f1, #4f46e5); color: white; border-radius: 8px; padding: 16px 24px; display: flex; justify-content: space-between; align-items: center; margin-top: 20px; }
                .total-label { font-size: 0.9rem; opacity: 0.9; }
                .total-value { font-size: 1.5rem; font-weight: 700; }
                .footer { background: #f8fafc; border-radius: 0 0 16px 16px; padding: 24px; text-align: center; color: #64748b; font-size: 0.85rem; }
                .footer strong { color: #6366f1; }
              </style>
            </head>
            <body>
              <div class="wrapper">
                <div class="header">
                  <div style="font-size:3rem;margin-bottom:12px">🎫</div>
                  <h1>Ingresso Confirmado!</h1>
                  <p>Olá, <strong>@@NOME_CLIENTE@@</strong>! Sua compra foi realizada com sucesso.</p>
                </div>
                <div class="body">
                  <p style="color:#64748b;margin-bottom:24px">
                    Seu ingresso digital está pronto. Apresente o código abaixo na entrada do evento.
                  </p>
                  <div class="ticket">
                    <div class="ticket-header">
                      <div class="ticket-emoji">🎭</div>
                      <div>
                        <p class="ticket-title">@@EVENTO_NOME@@</p>
                        <p class="ticket-subtitle">@@DATA_FORMATADA@@</p>
                      </div>
                    </div>
                    <div class="ticket-body">
                      <div class="info-grid">
                        <div class="info-item">
                          <label>Titular</label>
                          <span>@@NOME_CLIENTE@@</span>
                        </div>
                        <div class="info-item">
                          <label>Local</label>
                          <span>@@EVENTO_LOCAL@@</span>
                        </div>
                        <div class="info-item">
                          <label>Setor</label>
                          <span>@@SETOR_NOME@@</span>
                        </div>
                        <div class="info-item">
                          <label>Assentos</label>
                          <span>@@ASSENTOS@@</span>
                        </div>
                      </div>
                      <div class="codigo-box">
                        <div class="codigo-label">Código do Ingresso</div>
                        <div class="codigo">@@CODIGO_INGRESSO@@</div>
                        <div class="codigo-hint">Apresente este código na entrada do evento</div>
                      </div>
                      <div class="total-box">
                        <span class="total-label">Valor Total Pago</span>
                        <span class="total-value">@@VALOR_TOTAL@@</span>
                      </div>
                    </div>
                  </div>
                  <p style="color:#64748b;font-size:0.9rem;margin-top:24px">
                    ⚠️ Guarde este e-mail. Ele é seu comprovante de compra e ingresso de acesso ao evento.
                  </p>
                </div>
                <div class="footer">
                  <p>Compra realizada em @@DATA_COMPRA@@</p>
                  <p style="margin-top:8px">Dúvidas? Entre em contato com <strong>suporte@bilheteriavirtual.com</strong></p>
                  <p style="margin-top:16px;opacity:0.6">© @@ANO@@ Bilheteria Virtual — Unifeso</p>
                </div>
              </div>
            </body>
            </html>
            """;

        return template
            .Replace("@@NOME_CLIENTE@@", compra.NomeCliente)
            .Replace("@@EVENTO_NOME@@", compra.EventoNome)
            .Replace("@@EVENTO_LOCAL@@", compra.EventoLocal)
            .Replace("@@SETOR_NOME@@", compra.SetorNome)
            .Replace("@@ASSENTOS@@", assentos)
            .Replace("@@CODIGO_INGRESSO@@", compra.CodigoIngresso)
            .Replace("@@VALOR_TOTAL@@", compra.ValorTotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR")))
            .Replace("@@DATA_FORMATADA@@", dataFormatada)
            .Replace("@@DATA_COMPRA@@", compra.DataCompra.ToString("dd/MM/yyyy 'às' HH:mm"))
            .Replace("@@ANO@@", DateTime.Now.Year.ToString());
    }
}

public class EmailEnviado
{
    public int Id { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string CorpoHtml { get; set; } = string.Empty;
    public DateTime EnviadoEm { get; set; }
    public bool Sucesso { get; set; }
}
