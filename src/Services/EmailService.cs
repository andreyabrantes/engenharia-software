using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using QRCoder;

namespace BilheteriaAPI.Services;

public class EmailService(IConfiguration config)
{
    public async Task<bool> EnviarIngressoAsync(CompraResultado compra)
    {
        var smtpConfig = config.GetSection("Smtp");
        var host    = smtpConfig["Host"];
        var port    = int.Parse(smtpConfig["Port"] ?? "587");
        var user    = smtpConfig["User"];
        var pass    = smtpConfig["Pass"];
        var from    = smtpConfig["From"] ?? user;
        var enabled = bool.Parse(smtpConfig["Enabled"] ?? "false");

        Console.WriteLine($"[EMAIL] Para: {compra.EmailCliente} | Ingresso: {compra.CodigoIngresso}");

        if (!enabled || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
            return true;

        try
        {
            var qrBytes = GerarQrCodeBytes(compra);

            var builder  = new BodyBuilder();
            var qrLinked = builder.LinkedResources.Add("qrcode.png", qrBytes, ContentType.Parse("image/png"));
            qrLinked.ContentId          = MimeKit.Utils.MimeUtils.GenerateMessageId();
            qrLinked.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);

            builder.HtmlBody = GerarCorpoEmail(compra, qrLinked.ContentId);

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(compra.EmailCliente));
            message.Subject = $"🎫 Seu ingresso para {compra.EventoNome} chegou!";
            message.Body    = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(user, pass);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
            return false;
        }
    }

    private static byte[] GerarQrCodeBytes(CompraResultado compra)
    {
        var assentos = string.Join(", ", compra.NumerosAssentos);
        var conteudo =
            "INGRESSO BILHETERIA VIRTUAL\n" +
            "Codigo: " + compra.CodigoIngresso + "\n" +
            "Evento: " + compra.EventoNome + "\n" +
            "Data: " + compra.EventoData.ToString("dd/MM/yyyy HH:mm") + "\n" +
            "Local: " + compra.EventoLocal + "\n" +
            "Titular: " + compra.NomeCliente + "\n" +
            "Setor: " + compra.SetorNome + "\n" +
            "Assentos: " + assentos + "\n" +
            "Valor: " + compra.ValorTotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));

        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(8,
            darkColorRgba:  new byte[] { 0,   0,   0,   255 },
            lightColorRgba: new byte[] { 255, 255, 255, 255 });
    }

    private static string GerarCorpoEmail(CompraResultado compra, string qrCid)
    {
        var assentos       = string.Join(", ", compra.NumerosAssentos);
        var dataFormatada  = compra.EventoData.ToString("dddd, dd 'de' MMMM 'de' yyyy 'às' HH:mm", new System.Globalization.CultureInfo("pt-BR"));
        var valorFormatado = compra.ValorTotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
        var dataCompra     = compra.DataCompra.ToString("dd/MM/yyyy 'às' HH:mm");
        var ano            = DateTime.Now.Year.ToString();

        return
            "<!DOCTYPE html><html lang=\"pt-BR\"><head><meta charset=\"UTF-8\"/><style>" +
            "body{font-family:'Segoe UI',Arial,sans-serif;background:#f0f4ff;margin:0;padding:20px}" +
            ".wrapper{max-width:600px;margin:0 auto}" +
            ".header{background:linear-gradient(135deg,#6366f1,#ec4899);border-radius:16px 16px 0 0;padding:40px;text-align:center;color:white}" +
            ".header h1{margin:0;font-size:2rem}" +
            ".body{background:white;padding:32px}" +
            ".ticket{border:2px dashed #6366f1;border-radius:12px;overflow:hidden;margin:24px 0}" +
            ".ticket-header{background:linear-gradient(135deg,#6366f1,#ec4899);color:white;padding:20px 24px}" +
            ".ticket-body{padding:24px}" +
            ".info-grid{display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-bottom:20px}" +
            ".info-item label{display:block;font-size:.75rem;color:#64748b;text-transform:uppercase;font-weight:600;margin-bottom:4px}" +
            ".info-item span{font-weight:600;color:#1e293b}" +
            ".codigo-box{background:#f8fafc;border-radius:8px;padding:20px;text-align:center;border:1px solid #e2e8f0}" +
            ".codigo{font-size:1.4rem;font-weight:700;color:#6366f1;font-family:monospace;letter-spacing:.1em;margin:8px 0}" +
            ".qr-box{text-align:center;margin:16px 0;padding:12px;background:white}" +
            ".total-box{background:linear-gradient(135deg,#6366f1,#4f46e5);color:white;border-radius:8px;padding:16px 24px;display:flex;justify-content:space-between;align-items:center;margin-top:20px}" +
            ".footer{background:#f8fafc;border-radius:0 0 16px 16px;padding:24px;text-align:center;color:#64748b;font-size:.85rem}" +
            "</style></head><body><div class=\"wrapper\">" +
            "<div class=\"header\"><div style=\"font-size:3rem;margin-bottom:12px\">🎫</div>" +
            "<h1>Ingresso Confirmado!</h1>" +
            "<p>Olá, <strong>" + compra.NomeCliente + "</strong>! Sua compra foi realizada com sucesso.</p></div>" +
            "<div class=\"body\"><div class=\"ticket\">" +
            "<div class=\"ticket-header\">" +
            "<p style=\"font-size:1.3rem;font-weight:700;margin:0\">" + compra.EventoNome + "</p>" +
            "<p style=\"margin:4px 0 0;opacity:.9;font-size:.9rem\">" + dataFormatada + "</p></div>" +
            "<div class=\"ticket-body\"><div class=\"info-grid\">" +
            "<div class=\"info-item\"><label>Titular</label><span>" + compra.NomeCliente + "</span></div>" +
            "<div class=\"info-item\"><label>Local</label><span>" + compra.EventoLocal + "</span></div>" +
            "<div class=\"info-item\"><label>Setor</label><span>" + compra.SetorNome + "</span></div>" +
            "<div class=\"info-item\"><label>Assentos</label><span>" + assentos + "</span></div></div>" +
            "<div class=\"codigo-box\">" +
            "<div style=\"font-size:.8rem;color:#64748b;text-transform:uppercase;font-weight:600\">Código do Ingresso</div>" +
            "<div class=\"codigo\">" + compra.CodigoIngresso + "</div>" +
            "<div class=\"qr-box\">" +
            "<img src=\"cid:" + qrCid + "\" width=\"200\" height=\"200\" alt=\"QR Code\" style=\"display:block;margin:0 auto;border:4px solid white;border-radius:8px\"/>" +
            "<p style=\"font-size:.75rem;color:#64748b;margin-top:8px\">Escaneie para verificar o ingresso</p>" +
            "</div></div>" +
            "<div class=\"total-box\"><span>Valor Total Pago</span>" +
            "<span style=\"font-size:1.5rem;font-weight:700\">" + valorFormatado + "</span></div></div></div></div>" +
            "<div class=\"footer\">" +
            "<p>Compra realizada em " + dataCompra + "</p>" +
            "<p style=\"margin-top:8px\">Dúvidas? <strong>suporte@bilheteriavirtual.com</strong></p>" +
            "<p style=\"margin-top:16px;opacity:.6\">© " + ano + " Bilheteria Virtual — Unifeso</p>" +
            "</div></div></body></html>";
    }
}
