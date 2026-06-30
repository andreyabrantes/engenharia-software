using AutoMapper;
using BoraAli.Api.DTOs;
using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoraAli.Api.Services;

/// <summary>
/// Serviço responsável pela lógica de negócio de pedidos
/// </summary>
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly IDapperExecutor _dapper;
    private readonly IEmailService _emailService;

    private const int MaxTicketsPerEvent = 5;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger, IDapperExecutor dapper, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _dapper = dapper;
        _emailService = emailService;
    }

    public async Task<ApiResponseDto<OrderDto>> CreateOrderAsync(CreateOrderDto createDto, int userId)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetEventWithDetailsAsync(createDto.EventId);
            if (eventEntity == null) return ApiResponseDto<OrderDto>.Fail("Evento não encontrado");
            if (eventEntity.Status != "Published") return ApiResponseDto<OrderDto>.Fail("Evento não está disponível para compra");

            // ===== VALIDAÇÃO: Limite de 5 ingressos por evento por usuário =====
            var userTotalTickets = await GetUserTicketCountForEventAsync(userId, createDto.EventId);
            var requestedQuantity = createDto.Items.Sum(i => i.Quantity);

            if (userTotalTickets + requestedQuantity > MaxTicketsPerEvent)
            {
                var remaining = MaxTicketsPerEvent - userTotalTickets;
                return ApiResponseDto<OrderDto>.Fail(
                    string.Concat("Limite de ", MaxTicketsPerEvent.ToString(), " ingressos por evento. ",
                    "Você já possui ", userTotalTickets.ToString(), " ingresso(s) para este evento. ",
                    "Ainda pode comprar ", remaining.ToString(), " ingresso(s)."));
            }

            // ===== VALIDAÇÃO DE CUPOM DE DESCONTO =====
            decimal discountPercent = 0;
            Coupon? appliedCoupon = null;

            if (!string.IsNullOrWhiteSpace(createDto.CouponCode))
            {
                var couponResult = await ValidateCouponAsync(createDto.CouponCode, createDto.EventId);
                if (!couponResult.Success)
                    return ApiResponseDto<OrderDto>.Fail(couponResult.Message ?? "Cupom inválido");

                discountPercent = couponResult.Data!.DiscountPercent;
                // Obtem a entidade Coupon para incrementar CurrentUses apos a compra
                var coupons = await _unitOfWork.Coupons.FindAsync(c => c.Code == createDto.CouponCode.ToUpper().Trim());
                appliedCoupon = coupons.FirstOrDefault();
            }

            return await CreateOrderWithQuantityAsync(createDto, userId, eventEntity, discountPercent, appliedCoupon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido");
            return ApiResponseDto<OrderDto>.Fail("Erro ao criar pedido", new() { ex.Message });
        }
    }

    /// <summary>
    /// Cria pedido com quantidade (sem assentos nomeados) - CONCORRÊNCIA SEGURA
    /// </summary>
    private async Task<ApiResponseDto<OrderDto>> CreateOrderWithQuantityAsync(
        CreateOrderDto createDto, int userId, Event eventEntity,
        decimal discountPercent, Coupon? appliedCoupon)
    {
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemDto in createDto.Items)
        {
            var ticketType = eventEntity.TicketTypes.FirstOrDefault(t => t.Id == itemDto.TicketTypeId);
            if (ticketType == null)
                return ApiResponseDto<OrderDto>.Fail("Tipo de ingresso não encontrado");

            if (ticketType.AvailableQuantity < itemDto.Quantity)
                return ApiResponseDto<OrderDto>.Fail(string.Concat("Ingresso '", ticketType.Name, "' não possui quantidade suficiente"));

            var subtotal = ticketType.Price * itemDto.Quantity;
            totalAmount += subtotal;

            orderItems.Add(new OrderItem
            {
                TicketTypeId = itemDto.TicketTypeId,
                Quantity = itemDto.Quantity,
                UnitPrice = ticketType.Price,
                Subtotal = subtotal
            });
        }

        // Aplica desconto do cupom (porcentagem sobre o total)
        if (discountPercent > 0)
        {
            totalAmount = totalAmount * (1 - discountPercent / 100m);
        }

        // Inicia a transação
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,
                EventId = createDto.EventId,
                OrderCode = GenerateOrderCode(),
                TotalAmount = totalAmount,
                Status = "Pending",
                PaymentMethod = createDto.PaymentMethod,
                CreatedAt = DateTime.UtcNow
            };

            var createdOrder = await _unitOfWork.Orders.AddAsync(order);

            foreach (var item in orderItems)
            {
                item.OrderId = createdOrder.Id;
                await _unitOfWork.OrderItems.AddAsync(item);

                // ===== CONCORRÊNCIA SEGURA: UPDATE atômico com verificação =====
                string sql = "UPDATE TicketTypes SET AvailableQuantity = AvailableQuantity - @Qty WHERE Id = @TicketId AND AvailableQuantity >= @Qty";

                var affectedRows = await _dapper.ExecuteAsync(sql, new { Qty = item.Quantity, TicketId = item.TicketTypeId });

                if (affectedRows == 0)
                {
                    var ticketTypeName = eventEntity.TicketTypes
                        .FirstOrDefault(t => t.Id == item.TicketTypeId)?.Name ?? "desconhecido";
                    throw new Exception(string.Concat("Falha na compra: Os bilhetes do tipo '", ticketTypeName, "' esgotaram durante o processamento."));
                }
            }

            // Incrementa uso do cupom (dentro da transacao para atomicidade)
            if (appliedCoupon != null)
            {
                string couponSql = "UPDATE Coupons SET CurrentUses = CurrentUses + 1 WHERE Id = @Id AND CurrentUses < MaxUses";
                var couponUpdated = await _dapper.ExecuteAsync(couponSql, new { Id = appliedCoupon.Id });
                if (couponUpdated == 0)
                {
                    throw new Exception("O cupom expirou durante o processamento da compra.");
                }
            }

            // Confirma a transação
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Pedido {OrderCode} criado pelo usuário {UserId}", createdOrder.OrderCode, userId);

            // Envia e-mail de confirmação (não bloqueia o pedido se falhar)
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    await _emailService.SendTicketEmailAsync(
                        user.Email,
                        user.Name,
                        createdOrder.OrderCode,
                        eventEntity.Title,
                        eventEntity.EventDate.ToString("dd/MM/yyyy"),
                        eventEntity.Location);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar e-mail para pedido {OrderCode}", createdOrder.OrderCode);
            }

            var orderDto = _mapper.Map<OrderDto>(createdOrder);
            orderDto.Items = _mapper.Map<List<OrderItemDto>>(orderItems);
            return ApiResponseDto<OrderDto>.Ok(orderDto, "Pedido criado com sucesso");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Retorna a quantidade total de ingressos que o usuário já comprou para um evento
    /// </summary>
    private async Task<int> GetUserTicketCountForEventAsync(int userId, int eventId)
    {
        try
        {
            string sql = @"
                SELECT COALESCE(SUM(oi.Quantity), 0)
                FROM OrderItems oi
                INNER JOIN Orders o ON o.Id = oi.OrderId
                WHERE o.UserId = @UserId
                  AND o.EventId = @EventId
                  AND o.Status IN ('Pending', 'Confirmed')";

            return await _dapper.QuerySingleOrDefaultAsync<int>(sql, new { UserId = userId, EventId = eventId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao verificar limite de ingressos para usuário {UserId} no evento {EventId}", userId, eventId);
            return 0;
        }
    }

    public async Task<ApiResponseDto<OrderDto>> GetOrderByIdAsync(int id, int userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return ApiResponseDto<OrderDto>.Fail("Pedido não encontrado");
            if (order.UserId != userId) return ApiResponseDto<OrderDto>.Fail("Você não tem permissão para visualizar este pedido");

            var orderDto = _mapper.Map<OrderDto>(order);
            var items = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id);
            orderDto.Items = _mapper.Map<List<OrderItemDto>>(items);
            return ApiResponseDto<OrderDto>.Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedido {OrderId}", id);
            return ApiResponseDto<OrderDto>.Fail("Erro ao buscar pedido", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId)
    {
        try
        {
            // Usa Dapper com JOIN para buscar pedidos + dados do evento
            // (navigation properties não são carregadas pelo GenericRepository/Dapper)
            string sql = @"
                SELECT o.*, e.Title AS EventTitle, e.ImageUrl AS EventImageUrl,
                       e.EventDate AS EventDate, e.Location AS EventLocation
                FROM Orders o
                INNER JOIN Events e ON e.Id = o.EventId
                WHERE o.UserId = @UserId
                ORDER BY o.CreatedAt DESC";

            var orders = await _dapper.QueryAsync<OrderDto>(sql, new { UserId = userId });
            return ApiResponseDto<IEnumerable<OrderDto>>.Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos do usuário {UserId}", userId);
            return ApiResponseDto<IEnumerable<OrderDto>>.Fail("Erro ao buscar pedidos", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<bool>> CancelOrderAsync(int id, int userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return ApiResponseDto<bool>.Fail("Pedido não encontrado");
            if (order.UserId != userId) return ApiResponseDto<bool>.Fail("Você não tem permissão para cancelar este pedido");
            if (order.Status is "Cancelled" or "Refunded") return ApiResponseDto<bool>.Fail("Este pedido já foi cancelado");

            // Inicia transação para o cancelamento
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Orders.UpdateAsync(order);

                var items = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id);
                foreach (var item in items)
                {
                    // Devolve o stock do TicketType
                    string sql = "UPDATE TicketTypes SET AvailableQuantity = AvailableQuantity + @Qty WHERE Id = @TicketId";
                    await _dapper.ExecuteAsync(sql, new { Qty = item.Quantity, TicketId = item.TicketTypeId });
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Pedido {OrderId} cancelado pelo usuário {UserId}", id, userId);
                return ApiResponseDto<bool>.Ok(true, "Pedido cancelado com sucesso");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar pedido {OrderId}", id);
            return ApiResponseDto<bool>.Fail("Erro ao cancelar pedido", new() { ex.Message });
        }
    }

    /// <summary>
    /// Solicita reembolso de um pedido. Segue a regra de negócio:
    /// - Apenas o comprador pode solicitar o reembolso do próprio pedido
    /// - O pedido deve estar nos status "Pending" ou "Confirmed"
    /// - O evento ainda não pode ter acontecido (eventDate > DateTime.UtcNow)
    /// - Devolve os ingressos ao estoque e libera assentos
    /// </summary>
    public async Task<ApiResponseDto<bool>> RefundOrderAsync(int id, int userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return ApiResponseDto<bool>.Fail("Pedido não encontrado");
            if (order.UserId != userId) return ApiResponseDto<bool>.Fail("Você não tem permissão para reembolsar este pedido");

            // Regra de negócio: apenas pedidos Pending ou Confirmed podem ser reembolsados
            if (order.Status != "Pending" && order.Status != "Confirmed")
                return ApiResponseDto<bool>.Fail(
                    order.Status == "Refunded"
                        ? "Este pedido já foi reembolsado"
                        : order.Status == "Cancelled"
                            ? "Este pedido já foi cancelado"
                            : string.Concat("Não é possível reembolsar um pedido com status '", order.Status, "'"));

            // Regra de negócio: verificar se o evento ainda não aconteceu
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(order.EventId);
            if (eventEntity == null)
                return ApiResponseDto<bool>.Fail("Evento não encontrado");

            if (eventEntity.EventDate <= DateTime.UtcNow)
                return ApiResponseDto<bool>.Fail("Não é possível solicitar reembolso após a data do evento");

            // Inicia transação
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                order.Status = "Refunded";
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Orders.UpdateAsync(order);

                // Devolve os ingressos ao estoque
                var items = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id);
                foreach (var item in items)
                {
                    string stockSql = "UPDATE TicketTypes SET AvailableQuantity = AvailableQuantity + @Qty WHERE Id = @TicketId";
                    await _dapper.ExecuteAsync(stockSql, new { Qty = item.Quantity, TicketId = item.TicketTypeId });
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Pedido {OrderId} reembolsado para o usuário {UserId}. Evento: {EventTitle}",
                    id, userId, eventEntity.Title);
                return ApiResponseDto<bool>.Ok(true, "Reembolso solicitado com sucesso");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reembolsar pedido {OrderId}", id);
            return ApiResponseDto<bool>.Fail("Erro ao processar reembolso", new() { ex.Message });
        }
    }

    /// <summary>
    /// Confirma o pagamento de um pedido (simulação Pix), transicionando de Pending → Confirmed.
    /// Apenas o comprador pode confirmar o próprio pedido.
    /// </summary>
    public async Task<ApiResponseDto<bool>> ConfirmPaymentAsync(int id, int userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return ApiResponseDto<bool>.Fail("Pedido não encontrado");
            if (order.UserId != userId) return ApiResponseDto<bool>.Fail("Você não tem permissão para confirmar este pedido");
            if (order.Status != "Pending") return ApiResponseDto<bool>.Fail(string.Concat("Pedido com status '", order.Status, "' não pode ser confirmado"));

            order.Status = "Confirmed";
            order.ConfirmedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Orders.UpdateAsync(order);

            // Envia e-mail de confirmação (não bloqueia o pedido se falhar)
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                var ev = await _unitOfWork.Events.GetByIdAsync(order.EventId);
                if (user != null && ev != null)
                {
                    await _emailService.SendTicketEmailAsync(
                        user.Email,
                        user.Name,
                        order.OrderCode,
                        ev.Title,
                        ev.EventDate.ToString("dd/MM/yyyy"),
                        ev.Location);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar e-mail para pedido {OrderCode}", order.OrderCode);
            }

            _logger.LogInformation("Pagamento confirmado para pedido {OrderCode}", order.OrderCode);
            return ApiResponseDto<bool>.Ok(true, "Pagamento confirmado com sucesso! Seus ingressos estão garantidos.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar pagamento do pedido {OrderId}", id);
            return ApiResponseDto<bool>.Fail("Erro ao confirmar pagamento", new() { ex.Message });
        }
    }

    /// <summary>
    /// Check-in: valida um pedido pelo código e marca como "Used".
    /// Usado pelo organizador na entrada do evento para validar QR Codes.
    /// Só funciona para pedidos com status "Confirmed" e eventos que ainda não foram finalizados.
    /// </summary>
    public async Task<ApiResponseDto<OrderDto>> CheckInOrderAsync(string orderCode)
    {
        try
        {
            string sql = @"
                SELECT o.*, e.Title AS EventTitle, e.ImageUrl AS EventImageUrl,
                       e.EventDate AS EventDate, e.Location AS EventLocation
                FROM Orders o
                INNER JOIN Events e ON e.Id = o.EventId
                WHERE o.OrderCode = @OrderCode";

            var order = await _dapper.QuerySingleOrDefaultAsync<OrderDto>(sql, new { OrderCode = orderCode });
            if (order == null)
                return ApiResponseDto<OrderDto>.Fail("QR Code inválido. Pedido não encontrado.");

            if (order.Status == "Used")
                return ApiResponseDto<OrderDto>.Fail("Este ingresso já foi utilizado. Entrada já registrada.");

            if (order.Status != "Confirmed")
                return ApiResponseDto<OrderDto>.Fail(string.Concat("Este ingresso não pode ser utilizado (status: ", order.Status, "). O pagamento precisa estar confirmado."));

            // Marca como utilizado
            string updateSql = "UPDATE Orders SET Status = 'Used', UpdatedAt = @UpdatedAt WHERE Id = @Id AND Status = 'Confirmed'";
            var affected = await _dapper.ExecuteAsync(updateSql, new { UpdatedAt = DateTime.UtcNow, Id = order.Id });

            if (affected == 0)
                return ApiResponseDto<OrderDto>.Fail("Não foi possível registrar a entrada. Tente novamente.");

            order.Status = "Used";
            _logger.LogInformation("Check-in realizado para pedido {OrderCode} no evento {EventTitle}", orderCode, order.EventTitle);
            return ApiResponseDto<OrderDto>.Ok(order, "\u2705 Entrada Liberada! Bem-vindo ao evento!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer check-in do pedido {OrderCode}", orderCode);
            return ApiResponseDto<OrderDto>.Fail("Erro ao validar ingresso", new() { ex.Message });
        }
    }

    /// <summary>
    /// Valida um cupom de desconto e retorna as informações de desconto.
    /// Regras: cupom ativo, dentro do limite de usos, dentro da validade,
    /// e (se for específico) vinculado ao evento correto.
    /// </summary>
    public async Task<ApiResponseDto<CouponValidationDto>> ValidateCouponAsync(string code, int eventId)
    {
        try
        {
            var coupons = await _unitOfWork.Coupons.FindAsync(c => c.Code == code.ToUpper().Trim());
            var coupon = coupons.FirstOrDefault();

            if (coupon == null)
                return ApiResponseDto<CouponValidationDto>.Fail("Cupom inválido");

            if (!coupon.IsActive)
                return ApiResponseDto<CouponValidationDto>.Fail("Este cupom não está mais ativo");

            if (coupon.ValidUntil.HasValue && coupon.ValidUntil.Value < DateTime.UtcNow)
                return ApiResponseDto<CouponValidationDto>.Fail("Este cupom expirou");

            if (coupon.CurrentUses >= coupon.MaxUses)
                return ApiResponseDto<CouponValidationDto>.Fail("Este cupom já atingiu o limite máximo de usos");

            if (coupon.EventId.HasValue && coupon.EventId.Value != eventId)
                return ApiResponseDto<CouponValidationDto>.Fail("Este cupom não é válido para este evento");

            return ApiResponseDto<CouponValidationDto>.Ok(new CouponValidationDto
            {
                Code = coupon.Code,
                DiscountPercent = coupon.DiscountPercent,
                Description = coupon.Description,
                IsValid = true
            }, "Cupom aplicado com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar cupom {Code}", code);
            return ApiResponseDto<CouponValidationDto>.Fail("Erro ao validar cupom", new() { ex.Message });
        }
    }

    /// <summary>
    /// Exclui permanentemente um pedido do histórico do usuário.
    /// Permitido apenas para pedidos com status "Refunded" ou "Cancelled".
    /// </summary>
    public async Task<ApiResponseDto<bool>> DeleteOrderAsync(int id, int userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return ApiResponseDto<bool>.Fail("Pedido não encontrado");
            if (order.UserId != userId) return ApiResponseDto<bool>.Fail("Você não tem permissão para excluir este pedido");

            // Só permite excluir se já estiver reembolsado ou cancelado
            if (order.Status != "Refunded" && order.Status != "Cancelled")
                return ApiResponseDto<bool>.Fail("Só é possível excluir pedidos reembolsados ou cancelados");

            // Inicia transação
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Exclui os itens do pedido
                var items = await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id);
                foreach (var item in items)
                {
                    await _unitOfWork.OrderItems.DeleteAsync(item);
                }

                // Exclui o pedido
                await _unitOfWork.Orders.DeleteAsync(order);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Pedido {OrderId} excluído permanentemente pelo usuário {UserId}", id, userId);
                return ApiResponseDto<bool>.Ok(true, "Pedido excluído permanentemente");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir pedido {OrderId}", id);
            return ApiResponseDto<bool>.Fail("Erro ao excluir pedido", new() { ex.Message });
        }
    }

    private static string GenerateOrderCode()
    {
        var guidPart = Guid.NewGuid().ToString();
        return string.Concat("BA-", DateTime.UtcNow.ToString("yyyyMMdd"), "-", guidPart.Substring(0, 8).ToUpper());
    }
}
