using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using QRCoder;

namespace BilheteriaAPI.Services;

public class EmailService(IConfiguration config)
{
    private (string? host, int port, string? user, string? pass, string? from, bool enabled) LerConfig()
    {
        var s = config.GetSection("Smtp");
        return (
            s["Host"],
            int.Parse(s["Port"] ?? "587"),
            s["User"],
            s["Pass"],
            s["From"] ?? s["User"],
            bool.Parse(s["Enabled"] ?? "false")
        );
    }

    private async Task<bool> EnviarAsync(MimeMessage message)
    {
        var (host, port, user, pass, _, enabled) = LerConfig();
        if (!enabled || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
            return true;
        try
        {
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

    // -------------------------------------------------------------------------
    // CONFIRMAÇÃO DE COMPRA
    // -------------------------------------------------------------------------

    public async Task<bool> EnviarIngressoAsync(CompraResultado compra)
    {
        var (_, _, _, _, from, _) = LerConfig();
        Console.WriteLine($"[EMAIL] Ingresso para: {compra.EmailCliente} | {compra.CodigoIngresso}");

        var qrBytes  = GerarQrCodeBytes(compra.CodigoIngresso, compra.EventoNome, compra.NomeCliente);
        var builder  = new BodyBuilder();
        var qrLinked = builder.LinkedResources.Add("qrcode.png", qrBytes, ContentType.Parse("image/png"));
        qrLinked.ContentId          = MimeKit.Utils.MimeUtils.GenerateMessageId();
        qrLinked.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
        builder.HtmlBody = GerarHtmlIngresso(compra, qrLinked.ContentId);

        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(from));
        msg.To.Add(MailboxAddress.Parse(compra.EmailCliente));
        msg.Subject = $"🎫 Seu ingresso para {compra.EventoNome} chegou!";
        msg.Body    = builder.ToMessageBody();

        return await EnviarAsync(msg);
    }

    // -------------------------------------------------------------------------
    // CONFIRMAÇÃO DE REEMBOLSO
    // -------------------------------------------------------------------------

    public async Task<bool> EnviarReembolsoAsync(
        string emailCliente, string nomeCliente, string eventoNome,
        string codigoIngresso, List<string> assentos, decimal valorReembolsado, DateTime dataReembolso)
    {
        var (_, _, _, _, from, _) = LerConfig();
        Console.WriteLine($"[EMAIL] Reembolso para: {emailCliente} | {codigoIngresso}");

        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(from));
        msg.To.Add(MailboxAddress.Parse(emailCliente));
        msg.Subject = $"💸 Reembolso confirmado — {eventoNome}";

        var builder = new BodyBuilder { HtmlBody = GerarHtmlReembolso(nomeCliente, eventoNome, codigoIngresso, assentos, valorReembolsado, dataReembolso) };
        msg.Body = builder.ToMessageBody();

        return await EnviarAsync(msg);
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------

    private static byte[] GerarQrCodeBytes(string codigo, string evento, string titular)
    {
        var conteudo = $"BILHETERIA VIRTUAL\nCodigo: {codigo}\nEvento: {evento}\nTitular: {titular}";
        using var gen  = new QRCodeGenerator();
        var data       = gen.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.Q);
        using var qr   = new PngByteQRCode(data);
        return qr.GetGraphic(8,
            darkColorRgba:  new byte[] { 0, 0, 0, 255 },
            lightColorRgba: new byte[] { 255, 255, 255, 255 });
    }

    private static string GerarHtmlIngresso(CompraResultado c, string qrCid)
    {
        var assentos      = string.Join(", ", c.NumerosAssentos);
        var dataFormatada = c.EventoData.ToString("dddd, dd 'de' MMMM 'de' yyyy 'às' HH:mm", new System.Globalization.CultureInfo("pt-BR"));
        var valor         = c.ValorTotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
        var dataCompra    = c.DataCompra.ToString("dd/MM/yyyy 'às' HH:mm");

        return $@"<!DOCTYPE html><html lang=""pt-BR""><head><meta charset=""UTF-8""/><style>
body{{font-family:'Segoe UI',Arial,sans-serif;background:#f0f4ff;margin:0;padding:20px}}
.w{{max-width:600px;margin:0 auto}}
.hd{{background:linear-gradient(135deg,#6366f1,#ec4899);border-radius:16px 16px 0 0;padding:40px;text-align:center;color:white}}
.bd{{background:white;padding:32px}}
.tk{{border:2px dashed #6366f1;border-radius:12px;overflow:hidden;margin:24px 0}}
.tk-hd{{background:linear-gradient(135deg,#6366f1,#ec4899);color:white;padding:20px 24px}}
.tk-bd{{padding:24px}}
.grid{{display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-bottom:20px}}
.item label{{display:block;font-size:.75rem;color:#64748b;text-transform:uppercase;font-weight:600;margin-bottom:4px}}
.item span{{font-weight:600;color:#1e293b}}
.cod{{background:#f8fafc;border-radius:8px;padding:20px;text-align:center;border:1px solid #e2e8f0}}
.cod-val{{font-size:1.4rem;font-weight:700;color:#6366f1;font-family:monospace;letter-spacing:.1em;margin:8px 0}}
.total{{background:linear-gradient(135deg,#6366f1,#4f46e5);color:white;border-radius:8px;padding:16px 24px;display:flex;justify-content:space-between;align-items:center;margin-top:20px}}
.ft{{background:#f8fafc;border-radius:0 0 16px 16px;padding:24px;text-align:center;color:#64748b;font-size:.85rem}}
</style></head><body><div class=""w"">
<div class=""hd""><div style=""font-size:3rem;margin-bottom:12px"">🎫</div><h1 style=""margin:0"">Ingresso Confirmado!</h1>
<p>Olá, <strong>{c.NomeCliente}</strong>! Sua compra foi realizada com sucesso.</p></div>
<div class=""bd""><div class=""tk"">
<div class=""tk-hd""><p style=""font-size:1.3rem;font-weight:700;margin:0"">{c.EventoNome}</p>
<p style=""margin:4px 0 0;opacity:.9;font-size:.9rem"">{dataFormatada}</p></div>
<div class=""tk-bd""><div class=""grid"">
<div class=""item""><label>Titular</label><span>{c.NomeCliente}</span></div>
<div class=""item""><label>Local</label><span>{c.EventoLocal}</span></div>
<div class=""item""><label>Setor</label><span>{c.SetorNome}</span></div>
<div class=""item""><label>Assentos</label><span>{assentos}</span></div></div>
<div class=""cod""><div style=""font-size:.8rem;color:#64748b;text-transform:uppercase;font-weight:600"">Código do Ingresso</div>
<div class=""cod-val"">{c.CodigoIngresso}</div>
<div style=""text-align:center;margin:16px 0"">
<img src=""cid:{qrCid}"" width=""200"" height=""200"" alt=""QR Code"" style=""display:block;margin:0 auto;border:4px solid white;border-radius:8px""/>
<p style=""font-size:.75rem;color:#64748b;margin-top:8px"">Escaneie para verificar o ingresso</p></div></div>
<div class=""total""><span>Valor Total Pago</span><span style=""font-size:1.5rem;font-weight:700"">{valor}</span></div>
</div></div></div>
<div class=""ft""><p>Compra realizada em {dataCompra}</p>
<p style=""margin-top:8px"">Dúvidas? <strong>suporte@bilheteriavirtual.com</strong></p>
<p style=""margin-top:16px;opacity:.6"">© {DateTime.Now.Year} Bilheteria Virtual — Unifeso</p></div>
</div></body></html>";
    }

    private static string GerarHtmlReembolso(
        string nome, string evento, string codigo,
        List<string> assentos, decimal valor, DateTime dataReembolso)
    {
        var assentosStr = string.Join(", ", assentos);
        var valorStr    = valor.ToString("C2", new System.Globalization.CultureInfo("pt-BR"));
        var dataStr     = dataReembolso.ToString("dd/MM/yyyy 'às' HH:mm");

        return $@"<!DOCTYPE html><html lang=""pt-BR""><head><meta charset=""UTF-8""/><style>
body{{font-family:'Segoe UI',Arial,sans-serif;background:#f0f4ff;margin:0;padding:20px}}
.w{{max-width:600px;margin:0 auto}}
.hd{{background:linear-gradient(135deg,#dc2626,#f97316);border-radius:16px 16px 0 0;padding:40px;text-align:center;color:white}}
.bd{{background:white;padding:32px}}
.box{{border:2px dashed #dc2626;border-radius:12px;padding:24px;margin:24px 0}}
.grid{{display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-bottom:20px}}
.item label{{display:block;font-size:.75rem;color:#64748b;text-transform:uppercase;font-weight:600;margin-bottom:4px}}
.item span{{font-weight:600;color:#1e293b}}
.valor{{background:linear-gradient(135deg,#16a34a,#15803d);color:white;border-radius:8px;padding:16px 24px;display:flex;justify-content:space-between;align-items:center;margin-top:20px}}
.ft{{background:#f8fafc;border-radius:0 0 16px 16px;padding:24px;text-align:center;color:#64748b;font-size:.85rem}}
</style></head><body><div class=""w"">
<div class=""hd""><div style=""font-size:3rem;margin-bottom:12px"">💸</div><h1 style=""margin:0"">Reembolso Confirmado!</h1>
<p>Olá, <strong>{nome}</strong>! Seu reembolso foi processado com sucesso.</p></div>
<div class=""bd""><div class=""box"">
<div class=""grid"">
<div class=""item""><label>Evento</label><span>{evento}</span></div>
<div class=""item""><label>Assentos cancelados</label><span>{assentosStr}</span></div>
<div class=""item""><label>Código do ingresso</label><span style=""font-family:monospace"">{codigo}</span></div>
<div class=""item""><label>Data do reembolso</label><span>{dataStr}</span></div></div>
<div class=""valor""><span>Valor Reembolsado</span><span style=""font-size:1.5rem;font-weight:700"">{valorStr}</span></div>
</div>
<p style=""color:#64748b;font-size:.875rem"">O valor será estornado conforme a política do seu banco ou operadora de pagamento.</p>
</div>
<div class=""ft""><p>Dúvidas? <strong>suporte@bilheteriavirtual.com</strong></p>
<p style=""margin-top:16px;opacity:.6"">© {DateTime.Now.Year} Bilheteria Virtual — Unifeso</p></div>
</div></body></html>";
    }
}
