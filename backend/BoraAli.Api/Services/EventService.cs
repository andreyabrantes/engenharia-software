using AutoMapper;
using BoraAli.Api.DTOs;
using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoraAli.Api.Services;

/// <summary>
/// Serviço responsável pela lógica de negócio de eventos
/// </summary>
public class EventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EventService> _logger;
    private readonly IDapperExecutor _dapper;

    public EventService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EventService> logger, IDapperExecutor dapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _dapper = dapper;
    }

    public async Task<ApiResponseDto<PagedResultDto<EventDto>>> GetAllEventsAsync(
        int pageNumber = 1, int pageSize = 10, string? category = null,
        string? city = null, string? search = null, string? orderBy = null, bool ascending = true)
    {
        try
        {
            // Valida e sanitiza a coluna de ordenacao (protecao contra SQL injection)
            var validColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Id", "Title", "EventDate", "City", "CreatedAt", "Status"
            };
            var orderColumn = !string.IsNullOrEmpty(orderBy) && validColumns.Contains(orderBy)
                ? orderBy : "EventDate";
            var direction = ascending ? "ASC" : "DESC";
            var offset = (pageNumber - 1) * pageSize;

            // Constroi a clausula WHERE dinamicamente
            var whereClauses = new List<string> { "e.Status = 'Published'" };
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(search))
            {
                whereClauses.Add("(e.Title LIKE @Search OR e.Description LIKE @Search OR e.City LIKE @Search OR c.Name LIKE @Search)");
                parameters["Search"] = string.Concat("%", search, "%");
            }

            if (!string.IsNullOrEmpty(category))
            {
                whereClauses.Add("c.Slug = @CategorySlug");
                parameters["CategorySlug"] = category;
            }

            if (!string.IsNullOrEmpty(city))
            {
                whereClauses.Add("e.City = @City");
                parameters["City"] = city;
            }

            var whereSql = string.Join(" AND ", whereClauses);

            // COUNT total (apenas uma consulta ao banco)
            string countSql = string.Concat(
                "SELECT COUNT(*) FROM Events e ",
                "LEFT JOIN Categories c ON e.CategoryId = c.Id ",
                "WHERE ", whereSql);

            var totalCount = await _dapper.QuerySingleOrDefaultAsync<int>(countSql, parameters);

            // Query principal com JOINs e paginacao via LIMIT/OFFSET
            string dataSql = string.Concat(
                "SELECT e.*, c.*, u.*, t.* FROM Events e ",
                "LEFT JOIN Categories c ON e.CategoryId = c.Id ",
                "LEFT JOIN Users u ON e.OrganizerId = u.Id ",
                "LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1 ",
                "WHERE ", whereSql, " ",
                "ORDER BY e.[", orderColumn, "] ", direction, " ",
                "LIMIT @PageSize OFFSET @Offset");

            parameters["PageSize"] = pageSize;
            parameters["Offset"] = offset;

            // Mapeamento multi-entidade via Dapper (Event + Category + User + TicketType)
            var eventDict = new Dictionary<int, Event>();
            await _dapper.QueryMultiMapAsync<Event, Category, User, TicketType, Event>(
                dataSql,
                (ev, cat, usr, ticket) =>
                {
                    if (!eventDict.TryGetValue(ev.Id, out var existing))
                    {
                        existing = ev;
                        existing.Category = cat;
                        existing.Organizer = usr;
                        existing.TicketTypes = new List<TicketType>();
                        eventDict.Add(ev.Id, existing);
                    }
                    if (ticket != null)
                        existing.TicketTypes.Add(ticket);
                    return existing;
                },
                parameters,
                "Id,Id,Id");

            var events = eventDict.Values.ToList();
            var eventDtos = _mapper.Map<List<EventDto>>(events);

            return ApiResponseDto<PagedResultDto<EventDto>>.Ok(new PagedResultDto<EventDto>
            {
                Items = eventDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar eventos");
            return ApiResponseDto<PagedResultDto<EventDto>>.Fail("Erro ao buscar eventos", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<EventDto>> GetEventByIdAsync(int id)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetEventWithDetailsAsync(id);
            if (eventEntity == null) return ApiResponseDto<EventDto>.Fail("Evento não encontrado");
            return ApiResponseDto<EventDto>.Ok(_mapper.Map<EventDto>(eventEntity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evento {EventId}", id);
            return ApiResponseDto<EventDto>.Fail("Erro ao buscar evento", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<EventDto>> CreateEventAsync(CreateEventDto createDto, int organizerId)
    {
        try
        {
            var eventEntity = _mapper.Map<Event>(createDto);
            eventEntity.OrganizerId = organizerId;
            var created = await _unitOfWork.Events.AddAsync(eventEntity);

            foreach (var ticketDto in createDto.Tickets)
            {
                var ticket = _mapper.Map<TicketType>(ticketDto);
                ticket.EventId = created.Id;
                await _unitOfWork.TicketTypes.AddAsync(ticket);
            }

            var fullEvent = await _unitOfWork.Events.GetEventWithDetailsAsync(created.Id);
            _logger.LogInformation("Evento {EventId} criado pelo organizador {OrganizerId}", created.Id, organizerId);
            return ApiResponseDto<EventDto>.Ok(_mapper.Map<EventDto>(fullEvent), "Evento criado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar evento");
            return ApiResponseDto<EventDto>.Fail("Erro ao criar evento", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<EventDto>> UpdateEventAsync(int id, UpdateEventDto updateDto, int userId)
    {
        try
        {
            var existingEvent = await _unitOfWork.Events.GetByIdAsync(id);
            if (existingEvent == null) return ApiResponseDto<EventDto>.Fail("Evento não encontrado");
            if (existingEvent.OrganizerId != userId) return ApiResponseDto<EventDto>.Fail("Você não tem permissão para editar este evento");

            _mapper.Map(updateDto, existingEvent);
            await _unitOfWork.Events.UpdateAsync(existingEvent);

            var fullEvent = await _unitOfWork.Events.GetEventWithDetailsAsync(id);
            _logger.LogInformation("Evento {EventId} atualizado", id);
            return ApiResponseDto<EventDto>.Ok(_mapper.Map<EventDto>(fullEvent), "Evento atualizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar evento {EventId}", id);
            return ApiResponseDto<EventDto>.Fail("Erro ao atualizar evento", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteEventAsync(int id, int userId)
    {
        try
        {
            var existingEvent = await _unitOfWork.Events.GetByIdAsync(id);
            if (existingEvent == null) return ApiResponseDto<bool>.Fail("Evento não encontrado");
            if (existingEvent.OrganizerId != userId) return ApiResponseDto<bool>.Fail("Você não tem permissão para excluir este evento");

            await _unitOfWork.Events.DeleteAsync(existingEvent);
            _logger.LogInformation("Evento {EventId} excluído", id);
            return ApiResponseDto<bool>.Ok(true, "Evento excluído com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir evento {EventId}", id);
            return ApiResponseDto<bool>.Fail("Erro ao excluir evento", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<IEnumerable<EventDto>>> GetFeaturedEventsAsync()
    {
        try
        {
            var events = await _unitOfWork.Events.GetFeaturedEventsAsync();
            var eventDtos = _mapper.Map<List<EventDto>>(events.ToList());
            return ApiResponseDto<IEnumerable<EventDto>>.Ok(eventDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar eventos em destaque");
            return ApiResponseDto<IEnumerable<EventDto>>.Fail("Erro ao buscar eventos em destaque", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<IEnumerable<CategoryDto>>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return ApiResponseDto<IEnumerable<CategoryDto>>.Ok(_mapper.Map<IEnumerable<CategoryDto>>(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categorias");
            return ApiResponseDto<IEnumerable<CategoryDto>>.Fail("Erro ao buscar categorias", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<PagedResultDto<EventDto>>> GetMyEventsAsync(int organizerId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var offset = (pageNumber - 1) * pageSize;

            // COUNT total
            string countSql = "SELECT COUNT(*) FROM Events WHERE OrganizerId = @OrgId";
            var totalCount = await _dapper.QuerySingleOrDefaultAsync<int>(countSql, new { OrgId = organizerId });

            // Query principal com JOINs e paginacao
            string dataSql = @"
                SELECT e.*, c.*, u.*, t.*
                FROM Events e
                LEFT JOIN Categories c ON e.CategoryId = c.Id
                LEFT JOIN Users u ON e.OrganizerId = u.Id
                LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1
                WHERE e.OrganizerId = @OrgId
                ORDER BY e.CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            var eventDict = new Dictionary<int, Event>();
            await _dapper.QueryMultiMapAsync<Event, Category, User, TicketType, Event>(
                dataSql,
                (ev, cat, usr, ticket) =>
                {
                    if (!eventDict.TryGetValue(ev.Id, out var existing))
                    {
                        existing = ev;
                        existing.Category = cat;
                        existing.Organizer = usr;
                        existing.TicketTypes = new List<TicketType>();
                        eventDict.Add(ev.Id, existing);
                    }
                    if (ticket != null)
                        existing.TicketTypes.Add(ticket);
                    return existing;
                },
                new { OrgId = organizerId, PageSize = pageSize, Offset = offset },
                "Id,Id,Id");

            var eventDtos = _mapper.Map<List<EventDto>>(eventDict.Values.ToList());

            return ApiResponseDto<PagedResultDto<EventDto>>.Ok(new PagedResultDto<EventDto>
            { Items = eventDtos, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar eventos do organizador {OrganizerId}", organizerId);
            return ApiResponseDto<PagedResultDto<EventDto>>.Fail("Erro ao buscar eventos", new() { ex.Message });
        }
    }

    /// <summary>
    /// Publica um evento (transição Draft → Published) com validações de negócio.
    /// Regras de validação:
    /// 1. O evento deve estar no status "Draft"
    /// 2. A data do evento deve ser pelo menos 24 horas no futuro
    /// 3. O evento deve ter pelo menos 1 tipo de ingresso com quantidade positiva
    /// </summary>
    public async Task<ApiResponseDto<PublishEventResponseDto>> PublishEventAsync(int eventId, int userId)
    {
        try
        {
            var existingEvent = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (existingEvent == null)
                return ApiResponseDto<PublishEventResponseDto>.Fail("Evento não encontrado");

            // ===== VALIDAÇÃO 1: Somente o organizador dono pode publicar =====
            if (existingEvent.OrganizerId != userId)
                return ApiResponseDto<PublishEventResponseDto>.Fail("Você não tem permissão para publicar este evento");

            // ===== VALIDAÇÃO 2: O evento deve estar em Draft =====
            if (existingEvent.Status != "Draft")
                return ApiResponseDto<PublishEventResponseDto>.Fail(
                    string.Concat("Não é possível publicar um evento com status '", existingEvent.Status, "'. ",
                    "Apenas eventos em 'Draft' podem ser publicados."));

            // ===== VALIDAÇÃO 3: Data do evento deve ser pelo menos 24h no futuro =====
            var minimumDate = DateTime.UtcNow.AddHours(24);
            if (existingEvent.EventDate <= minimumDate)
                return ApiResponseDto<PublishEventResponseDto>.Fail(
                    string.Concat("O evento deve ocorrer com pelo menos 24 horas de antecedência. ",
                    "Data do evento: ", existingEvent.EventDate.ToString("dd/MM/yyyy HH:mm"), ". ",
                    "Data mínima exigida: ", minimumDate.ToString("dd/MM/yyyy HH:mm"), "."));

            // ===== VALIDAÇÃO 4: Deve ter pelo menos 1 tipo de ingresso ativo com quantidade > 0 =====
            var ticketTypes = await _unitOfWork.TicketTypes.FindAsync(t => t.EventId == eventId && t.IsActive);
            var activeTickets = ticketTypes.ToList();
            if (!activeTickets.Any())
                return ApiResponseDto<PublishEventResponseDto>.Fail(
                    "O evento precisa ter pelo menos um tipo de ingresso ativo para ser publicado.");

            if (activeTickets.All(t => t.TotalQuantity <= 0))
                return ApiResponseDto<PublishEventResponseDto>.Fail(
                    "Pelo menos um tipo de ingresso deve ter quantidade positiva para publicar o evento.");

            // Atualiza o status para Published
            existingEvent.Status = "Published";
            existingEvent.PublishedAt = DateTime.UtcNow;
            existingEvent.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Events.UpdateAsync(existingEvent);

            _logger.LogInformation("Evento {EventId} publicado pelo organizador {OrganizerId}", eventId, userId);

            return ApiResponseDto<PublishEventResponseDto>.Ok(new PublishEventResponseDto
            {
                EventId = existingEvent.Id,
                Title = existingEvent.Title,
                Status = existingEvent.Status,
                PublishedAt = existingEvent.PublishedAt.Value,
                IsPublished = true
            }, "Evento publicado com sucesso! Agora ele está visível para o público.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento {EventId}", eventId);
            return ApiResponseDto<PublishEventResponseDto>.Fail("Erro ao publicar evento", new() { ex.Message });
        }
    }

    /// <summary>
    /// Obtém resumo de vendas de um evento usando INNER JOIN e LEFT JOIN via Dapper.
    /// Cruza as tabelas Events, Orders, OrderItems e TicketTypes com agregações (SUM, COUNT).
    /// </summary>
    public async Task<ApiResponseDto<EventSalesSummaryDto>> GetEventSalesSummaryAsync(int eventId, int userId)
    {
        try
        {
            // Verifica se o evento existe e pertence ao organizador
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                return ApiResponseDto<EventSalesSummaryDto>.Fail("Evento não encontrado");
            if (eventEntity.OrganizerId != userId)
                return ApiResponseDto<EventSalesSummaryDto>.Fail("Você não tem permissão para visualizar este resumo");

            // ===== CONSULTA COM INNER JOIN: Vendas confirmadas + usadas =====
            string summarySql = @"
                SELECT
                    COALESCE(SUM(o.TotalAmount), 0) AS TotalRevenue,
                    COALESCE(SUM(oi.Quantity), 0) AS TotalTicketsSold,
                    COUNT(DISTINCT o.Id) AS TotalOrders,
                    COUNT(DISTINCT CASE WHEN o.Status = 'Confirmed' THEN o.Id END) AS ConfirmedOrders,
                    COUNT(DISTINCT CASE WHEN o.Status = 'Pending' THEN o.Id END) AS PendingOrders
                FROM Orders o
                INNER JOIN OrderItems oi ON oi.OrderId = o.Id
                WHERE o.EventId = @EventId
                  AND o.Status IN ('Confirmed', 'Used', 'Pending')";

            var summary = await _dapper.QuerySingleOrDefaultAsync<dynamic>(summarySql, new { EventId = eventId });

            // ===== CONSULTA COM LEFT JOIN: Total de ingressos disponíveis =====
            string availableSql = @"
                SELECT COALESCE(SUM(AvailableQuantity), 0)
                FROM TicketTypes
                WHERE EventId = @EventId AND IsActive = 1";

            int totalAvailable = await _dapper.QuerySingleOrDefaultAsync<int>(availableSql, new { EventId = eventId });

            // ===== CONSULTA COM INNER JOIN + LEFT JOIN: Vendas por tipo de ingresso =====
            string byTypeSql = @"
                SELECT
                    tt.Id AS TicketTypeId,
                    tt.Name AS TicketName,
                    tt.Price,
                    tt.AvailableQuantity AS Available,
                    COALESCE(SUM(oi.Quantity), 0) AS Sold,
                    COALESCE(SUM(oi.Subtotal), 0) AS Revenue
                FROM TicketTypes tt
                LEFT JOIN OrderItems oi ON oi.TicketTypeId = tt.Id
                LEFT JOIN Orders o ON o.Id = oi.OrderId AND o.Status IN ('Confirmed', 'Used')
                WHERE tt.EventId = @EventId AND tt.IsActive = 1
                GROUP BY tt.Id, tt.Name, tt.Price, tt.AvailableQuantity
                ORDER BY tt.Name";

            var byType = await _dapper.QueryAsync<TicketTypeSalesDto>(byTypeSql, new { EventId = eventId });

            // Calcula a taxa de ocupação
            int totalSold = (int)(summary?.TotalTicketsSold ?? 0);
            int totalAllTickets = totalAvailable + totalSold;
            double occupancyRate = totalAllTickets > 0
                ? Math.Round((double)totalSold / totalAllTickets * 100, 2)
                : 0;

            var salesSummary = new EventSalesSummaryDto
            {
                EventId = eventId,
                EventTitle = eventEntity.Title,
                EventDate = eventEntity.EventDate,
                TotalRevenue = (decimal)(summary?.TotalRevenue ?? 0),
                TotalTicketsSold = totalSold,
                TotalTicketsAvailable = totalAvailable,
                OccupancyRate = occupancyRate,
                TotalOrders = (int)(summary?.TotalOrders ?? 0),
                ConfirmedOrders = (int)(summary?.ConfirmedOrders ?? 0),
                PendingOrders = (int)(summary?.PendingOrders ?? 0),
                SalesByTicketType = byType.ToList()
            };

            _logger.LogInformation("Resumo de vendas do evento {EventId} consultado pelo organizador {UserId}", eventId, userId);

            return ApiResponseDto<EventSalesSummaryDto>.Ok(salesSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar resumo de vendas do evento {EventId}", eventId);
            return ApiResponseDto<EventSalesSummaryDto>.Fail("Erro ao buscar resumo de vendas", new() { ex.Message });
        }
    }

    /// <summary>
    /// Obtém estatísticas agregadas do organizador usando SUM, COUNT e GROUP BY (Dapper).
    /// Usado pelo dashboard analítico na página "Meus Eventos".
    /// </summary>
    public async Task<ApiResponseDto<OrganizerStatsDto>> GetOrganizerStatsAsync(int organizerId)
    {
        try
        {
            var stats = new OrganizerStatsDto();

            // Total de eventos do organizador
            string eventsCountSql = "SELECT COUNT(*) FROM Events WHERE OrganizerId = @OrgId";
            stats.TotalEvents = await _dapper.QuerySingleOrDefaultAsync<int>(eventsCountSql, new { OrgId = organizerId });

            // Receita total e ingressos vendidos (pedidos confirmados/usados)
            string revenueSql = @"
                SELECT
                    COALESCE(SUM(o.TotalAmount), 0) AS TotalRevenue,
                    COALESCE(SUM(oi.Quantity), 0) AS TotalSold,
                    COUNT(DISTINCT o.Id) AS TotalOrders
                FROM Orders o
                INNER JOIN OrderItems oi ON oi.OrderId = o.Id
                INNER JOIN Events e ON e.Id = o.EventId
                WHERE e.OrganizerId = @OrgId
                  AND o.Status IN ('Confirmed', 'Used')";

            var revenueData = await _dapper.QuerySingleOrDefaultAsync<dynamic>(revenueSql, new { OrgId = organizerId });
            stats.TotalRevenue = (decimal)(revenueData?.TotalRevenue ?? 0);
            stats.TotalTicketsSold = (int)(revenueData?.TotalSold ?? 0);
            stats.TotalOrders = (int)(revenueData?.TotalOrders ?? 0);

            // Total de ingressos disponíveis
            string availableSql = "SELECT COALESCE(SUM(TotalQuantity), 0) FROM TicketTypes WHERE EventId IN (SELECT Id FROM Events WHERE OrganizerId = @OrgId)";
            stats.TotalTicketsAvailable = await _dapper.QuerySingleOrDefaultAsync<int>(availableSql, new { OrgId = organizerId });

            // Receita por tipo de ingresso (GROUP BY)
            string byTypeSql = @"
                SELECT
                    tt.Name,
                    SUM(oi.Quantity) AS Sold,
                    SUM(oi.Subtotal) AS Revenue
                FROM OrderItems oi
                INNER JOIN TicketTypes tt ON tt.Id = oi.TicketTypeId
                INNER JOIN Orders o ON o.Id = oi.OrderId
                INNER JOIN Events e ON e.Id = o.EventId
                WHERE e.OrganizerId = @OrgId
                  AND o.Status IN ('Confirmed', 'Used')
                GROUP BY tt.Name
                ORDER BY Revenue DESC";

            var byType = await _dapper.QueryAsync<TicketTypeStatDto>(byTypeSql, new { OrgId = organizerId });
            stats.RevenueByTicketType = byType.ToList();

            // Receita por evento (GROUP BY)
            string byEventSql = @"
                SELECT
                    e.Id AS EventId,
                    e.Title AS Title,
                    SUM(oi.Quantity) AS TicketsSold,
                    SUM(o.TotalAmount) AS Revenue
                FROM Orders o
                INNER JOIN OrderItems oi ON oi.OrderId = o.Id
                INNER JOIN Events e ON e.Id = o.EventId
                WHERE e.OrganizerId = @OrgId
                  AND o.Status IN ('Confirmed', 'Used')
                GROUP BY e.Id, e.Title
                ORDER BY Revenue DESC";

            var byEvent = await _dapper.QueryAsync<EventStatDto>(byEventSql, new { OrgId = organizerId });
            stats.RevenueByEvent = byEvent.ToList();

            return ApiResponseDto<OrganizerStatsDto>.Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas do organizador {OrganizerId}", organizerId);
            return ApiResponseDto<OrganizerStatsDto>.Fail("Erro ao buscar estatísticas", new() { ex.Message });
        }
    }
}
