using System.Security.Claims;
using BoraAli.Api.DTOs;
using BoraAli.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BoraAli.Api.Controllers;

/// <summary>
/// Controller responsável por operações relacionadas a pedidos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Policy = "ClienteOrOrganizador")]
[EnableRateLimiting("Global")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<OrderDto>>> Create([FromBody] CreateOrderDto createDto)
    {
        var userId = GetUserId();
        var result = await _orderService.CreateOrderAsync(createDto, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Obtém um pedido pelo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<OrderDto>>> GetById(int id)
    {
        var userId = GetUserId();
        var result = await _orderService.GetOrderByIdAsync(id, userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os pedidos do usuário autenticado
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<OrderDto>>>> GetMyOrders()
    {
        var userId = GetUserId();
        var result = await _orderService.GetUserOrdersAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Cancela um pedido
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Cancel(int id)
    {
        var userId = GetUserId();
        var result = await _orderService.CancelOrderAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Solicita reembolso de um pedido.
    /// Regra de negócio: só é permitido se o evento ainda não aconteceu
    /// e o pedido estiver nos status "Pending" ou "Confirmed".
    /// </summary>
    [HttpPost("{id}/refund")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Refund(int id)
    {
        var userId = GetUserId();
        var result = await _orderService.RefundOrderAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Confirma pagamento de um pedido (simulação Pix).
    /// Transiciona o pedido de "Pending" para "Confirmed".
    /// </summary>
    [HttpPost("{id}/confirm-payment")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ConfirmPayment(int id)
    {
        var userId = GetUserId();
        var result = await _orderService.ConfirmPaymentAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Check-in: valida um QR Code de ingresso e marca como "Used".
    /// Aberto para organizadores fazerem a validação na entrada do evento.
    /// </summary>
    [HttpPost("checkin")]
    [Authorize(Policy = "OrganizadorOnly")]
    [EnableRateLimiting("CheckIn")]
    public async Task<ActionResult<ApiResponseDto<OrderDto>>> CheckIn([FromBody] CheckInRequestDto request)
    {
        var result = await _orderService.CheckInOrderAsync(request.OrderCode);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Check-in público (sem autenticação) via QR Code — usado pela página /checkin
    /// </summary>
    [HttpPost("public-checkin")]
    [AllowAnonymous]
    [EnableRateLimiting("CheckIn")]
    public async Task<ActionResult<ApiResponseDto<OrderDto>>> PublicCheckIn([FromBody] CheckInRequestDto request)
    {
        var result = await _orderService.CheckInOrderAsync(request.OrderCode);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Valida um cupom de desconto para um evento
    /// </summary>
    [HttpPost("validate-coupon")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<CouponValidationDto>>> ValidateCoupon([FromBody] ValidateCouponRequestDto request)
    {
        var result = await _orderService.ValidateCouponAsync(request.Code, request.EventId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Exclui/remove um pedido do histórico do usuário (apenas reembolsados ou cancelados).
    /// Literalmente deleta o registro do banco de dados.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(int id)
    {
        var userId = GetUserId();
        var result = await _orderService.DeleteOrderAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : 0;
    }
}

