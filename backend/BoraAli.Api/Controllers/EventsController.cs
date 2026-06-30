using System.Security.Claims;
using System.Text.Json;
using BoraAli.Api.DTOs;
using BoraAli.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BoraAli.Api.Controllers;

/// <summary>
/// Controller responsável por operações relacionadas a eventos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("Global")]
public class EventsController : ControllerBase
{
    private readonly EventService _eventService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EventsController> _logger;

    public EventsController(EventService eventService, IWebHostEnvironment env, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os eventos com suporte a filtros, paginação e ordenação
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<EventDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool ascending = true)
    {
        var result = await _eventService.GetAllEventsAsync(pageNumber, pageSize, category, city, search, orderBy, ascending);
        return Ok(result);
    }

    /// <summary>
    /// Obtém eventos em destaque
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<EventDto>>>> GetFeatured()
    {
        var result = await _eventService.GetFeaturedEventsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtém todas as categorias
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<CategoryDto>>>> GetCategories()
    {
        var result = await _eventService.GetCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtém um evento pelo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> GetById(int id)
    {
        var result = await _eventService.GetEventByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Cria um novo evento via JSON (requer autenticação como Organizador)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "OrganizadorOnly")]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> Create([FromBody] CreateEventDto createDto)
    {
        var userId = GetUserId();
        var result = await _eventService.CreateEventAsync(createDto, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Cria um novo evento com upload de imagem (requer autenticação como Organizador) - Aceita multipart/form-data
    /// </summary>
    [HttpPost("with-image")]
    [Authorize(Policy = "OrganizadorOnly")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> CreateWithImage([FromForm] CreateEventFormDto formDto)
    {
        var userId = GetUserId();

        // Converte FormDto para CreateEventDto
        var createDto = new CreateEventDto
        {
            Title = formDto.Title,
            Description = formDto.Description,
            FullDescription = formDto.FullDescription,
            EventDate = formDto.EventDate,
            Time = formDto.Time,
            Location = formDto.Location,
            Address = formDto.Address,
            City = formDto.City,
            Cep = formDto.Cep,
            Street = formDto.Street,
            Neighborhood = formDto.Neighborhood,
            State = formDto.State,
            AddressNumber = formDto.AddressNumber,
            CategoryId = formDto.CategoryId,
        };

        // Processa tickets do JSON
        if (!string.IsNullOrEmpty(formDto.TicketsJson))
        {
            try
            {
                createDto.Tickets = JsonSerializer.Deserialize<List<CreateTicketTypeDto>>(formDto.TicketsJson) ?? new();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao desserializar tickets");
                return BadRequest(ApiResponseDto<EventDto>.Fail("Formato inválido dos tickets"));
            }
        }

        // Processa upload da imagem com validacao de seguranca
        if (formDto.Image != null && formDto.Image.Length > 0)
        {
            // ===== VALIDACAO: Tamanho maximo 5 MB =====
            const long MaxFileSize = 5 * 1024 * 1024;
            if (formDto.Image.Length > MaxFileSize)
                return BadRequest(ApiResponseDto<EventDto>.Fail(
                    "A imagem deve ter no maximo 5 MB."));

            // ===== VALIDACAO: Extensoes permitidas =====
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };
            var extension = Path.GetExtension(formDto.Image.FileName);
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                return BadRequest(ApiResponseDto<EventDto>.Fail(
                    "Formato de imagem nao permitido. Use: .jpg, .jpeg, .png ou .webp."));

            // ===== VALIDACAO: Content-Type (MIME) permitido =====
            var allowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg", "image/png", "image/webp"
            };
            if (!allowedMimeTypes.Contains(formDto.Image.ContentType))
                return BadRequest(ApiResponseDto<EventDto>.Fail(
                    "Tipo de arquivo invalido. Envie uma imagem JPEG, PNG ou WebP."));

            try
            {
                var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "events");
                Directory.CreateDirectory(uploadsDir);

                var fileName = string.Concat(Guid.NewGuid().ToString(), extension);
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formDto.Image.CopyToAsync(stream);
                }

                createDto.ImageUrl = "/uploads/events/" + fileName;
                _logger.LogInformation("Imagem do evento salva em {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar imagem do evento");
                return BadRequest(ApiResponseDto<EventDto>.Fail("Erro ao fazer upload da imagem"));
            }
        }

        var result = await _eventService.CreateEventAsync(createDto, userId);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Atualiza um evento existente (requer autenticação)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "OrganizadorOnly")]
    public async Task<ActionResult<ApiResponseDto<EventDto>>> Update(int id, [FromBody] UpdateEventDto updateDto)
    {
        var userId = GetUserId();
        var result = await _eventService.UpdateEventAsync(id, updateDto, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Exclui um evento (requer autenticação)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "OrganizadorOnly")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(int id)
    {
        var userId = GetUserId();
        var result = await _eventService.DeleteEventAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Lista eventos do organizador autenticado
    /// </summary>
    [HttpGet("my-events")]
    [Authorize(Policy = "OrganizadorOnly")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<EventDto>>>> GetMyEvents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var result = await _eventService.GetMyEventsAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Obtém estatísticas agregadas do organizador para o dashboard analítico
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Policy = "OrganizadorOnly")]
    public async Task<ActionResult<ApiResponseDto<OrganizerStatsDto>>> GetStats()
    {
        var userId = GetUserId();
        var result = await _eventService.GetOrganizerStatsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Publica um evento (Draft → Published) com validações de negócio.
    /// Regras: status Draft, data mínima 24h futura, ao menos 1 ingresso ativo.
    /// </summary>
    [HttpPost("{id}/publish")]
    [Authorize(Policy = "OrganizadorOnly")]
    public async Task<ActionResult<ApiResponseDto<PublishEventResponseDto>>> Publish(int id)
    {
        var userId = GetUserId();
        var result = await _eventService.PublishEventAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Obtém resumo de vendas de um evento com JOINs (Orders, OrderItems, TicketTypes).
    /// Inclui receita total, taxa de ocupação e vendas por tipo de ingresso.
    /// </summary>
    [HttpGet("{id}/sales-summary")]
    [Authorize(Policy = "OrganizadorOnly")]
    public async Task<ActionResult<ApiResponseDto<EventSalesSummaryDto>>> GetSalesSummary(int id)
    {
        var userId = GetUserId();
        var result = await _eventService.GetEventSalesSummaryAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : 0;
    }
}
