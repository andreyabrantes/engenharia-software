namespace BoraAli.Core.Interfaces;

/// <summary>
/// Serviço de envio de e-mails transacionais (confirmação de compra, ingressos).
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia o e-mail de confirmação de compra com os detalhes do ingresso.
    /// </summary>
    /// <param name="toEmail">E-mail do destinatário (comprador)</param>
    /// <param name="customerName">Nome do comprador</param>
    /// <param name="orderCode">Código do pedido (ex: BA-20260615-A1B2C3D4)</param>
    /// <param name="eventTitle">Título do evento</param>
    /// <param name="eventDate">Data do evento (formato legível)</param>
    /// <param name="eventLocation">Local do evento</param>
    Task SendTicketEmailAsync(
        string toEmail,
        string customerName,
        string orderCode,
        string eventTitle,
        string eventDate,
        string eventLocation);
}
