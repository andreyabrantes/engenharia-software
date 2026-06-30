using System.Net;
using System.Net.Mail;
using BoraAli.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BoraAli.Api.Services;

/// <summary>
/// Serviço de envio de e-mails transacionais usando SMTP (Gmail).
/// Implementa <see cref="IEmailService"/> usando <see cref="SmtpClient"/> nativo do .NET.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendTicketEmailAsync(
        string toEmail,
        string customerName,
        string orderCode,
        string eventTitle,
        string eventDate,
        string eventLocation)
    {
        var settings = _configuration.GetSection("EmailSettings");
        var smtpServer = settings["SmtpServer"];
        var port = settings.GetValue<int>("Port");
        var senderEmail = settings["SenderEmail"];
        var senderName = settings["SenderName"];
        var username = settings["Username"];
        var password = settings["Password"];

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(password))
        {
            _logger.LogInformation(
                "Envio de e-mail não configurado (SmtpServer ou Password ausente). " +
                "Pedido {OrderCode} confirmado sem notificação por e-mail.",
                orderCode);
            return;
        }

        using var mail = new MailMessage
        {
            From = new MailAddress(senderEmail!, senderName),
            Subject = $"✅ Seus ingressos para {eventTitle} estão confirmados! (Pedido #{orderCode})",
            IsBodyHtml = true,
            Body = BuildHtmlBody(customerName, orderCode, eventTitle, eventDate, eventLocation),
        };

        mail.To.Add(new MailAddress(toEmail, customerName));

        using var smtp = new SmtpClient(smtpServer, port)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password),
        };

        try
        {
            await smtp.SendMailAsync(mail);
            _logger.LogInformation(
                "E-mail de confirmação enviado para {Email} — Pedido {OrderCode} — Evento: {EventTitle}",
                toEmail, orderCode, eventTitle);
        }
        catch (SmtpException ex)
        {
            _logger.LogWarning(ex,
                "Falha SMTP ao enviar e-mail para {Email} (Pedido {OrderCode}). " +
                "Verifique se a senha de app do Gmail está correta e se a verificação em 2 etapas está ativada.",
                toEmail, orderCode);
            throw;
        }
    }

    /// <summary>
    /// Template HTML simples e elegante para o corpo do e-mail de confirmação.
    /// </summary>
    private static string BuildHtmlBody(
        string customerName,
        string orderCode,
        string eventTitle,
        string eventDate,
        string eventLocation)
    {
        var firstName = customerName.Split(' ')[0];

        return $"""
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>
<body style="margin:0;padding:0;background-color:#f4f4f5;font-family:Arial,Helvetica,sans-serif">
    <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f5;padding:40px 0">
        <tr>
            <td align="center">
                <table width="560" cellpadding="0" cellspacing="0"
                       style="background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08)">

                    <!-- Header -->
                    <tr>
                        <td style="background:linear-gradient(135deg,#7c3aed,#a855f7);padding:32px 40px;text-align:center">
                            <h1 style="color:#ffffff;font-size:24px;margin:0 0 4px 0">🎟️ BoraAli</h1>
                            <p style="color:#e9d5ff;font-size:14px;margin:0">Compra Confirmada!</p>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style="padding:32px 40px">

                            <p style="font-size:16px;color:#18181b;margin:0 0 8px 0">
                                Olá, <strong>{customerName}</strong>!
                            </p>
                            <p style="font-size:14px;color:#52525b;line-height:1.6;margin:0 0 24px 0">
                                Seu pedido <strong style="color:#7c3aed">#{orderCode}</strong> foi confirmado com sucesso.
                                Seus ingressos estão garantidos! 🎉
                            </p>

                            <!-- Event Details Card -->
                            <table width="100%" cellpadding="0" cellspacing="0"
                                   style="background-color:#fafafa;border:1px solid #e4e4e7;border-radius:8px;margin-bottom:24px">
                                <tr>
                                    <td style="padding:20px 24px">
                                        <p style="font-size:12px;color:#a1a1aa;text-transform:uppercase;letter-spacing:0.5px;margin:0 0 12px 0">
                                            Detalhes do Evento
                                        </p>
                                        <p style="font-size:18px;font-weight:700;color:#18181b;margin:0 0 8px 0">
                                            {eventTitle}
                                        </p>
                                        <table cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td style="padding:4px 0;font-size:14px;color:#52525b">
                                                    📅 <strong>Data:</strong> {eventDate}
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="padding:4px 0;font-size:14px;color:#52525b">
                                                    📍 <strong>Local:</strong> {eventLocation}
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- QR Code Info -->
                            <table width="100%" cellpadding="0" cellspacing="0"
                                   style="background-color:#fefce8;border:1px solid #fde68a;border-radius:8px;margin-bottom:24px">
                                <tr>
                                    <td style="padding:16px 20px">
                                        <p style="font-size:14px;color:#854d0e;margin:0">
                                            📱 O <strong>QR Code</strong> para check-in está disponível na seção
                                            <strong>"Meus Pedidos"</strong> do site BoraAli.
                                            Apresente-o na entrada do evento.
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <p style="font-size:13px;color:#a1a1aa;line-height:1.5;margin:0">
                                Em caso de dúvidas, responda este e-mail ou acesse
                                <a href="http://localhost:3000/meus-pedidos" style="color:#7c3aed;text-decoration:none">
                                    Meus Pedidos
                                </a>.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#fafafa;padding:20px 40px;text-align:center;border-top:1px solid #e4e4e7">
                            <p style="font-size:12px;color:#a1a1aa;margin:0">
                                Equipe <strong style="color:#7c3aed">BoraAli</strong> —
                                Seus eventos, seus ingressos, do seu jeito.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
""";
    }
}
