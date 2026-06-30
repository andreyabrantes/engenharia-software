using System.Security.Claims;
using BoraAli.Api.DTOs;
using BoraAli.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoraAli.Api.Controllers;

/// <summary>
/// Controller para gerenciar favoritos de eventos e seguir organizadores
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly FavoriteService _favoriteService;

    public FavoritesController(FavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ==================== EVENTOS FAVORITOS ====================

    /// <summary>
    /// Alterna o estado de favorito de um evento (favoritar/desfavoritar)
    /// </summary>
    [HttpPost("events/{eventId}/toggle")]
    public async Task<IActionResult> ToggleEventFavorite(int eventId)
    {
        var userId = GetUserId();
        var result = await _favoriteService.ToggleEventFavoriteAsync(eventId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Verifica se um evento é favorito do usuário logado
    /// </summary>
    [HttpGet("events/{eventId}/status")]
    public async Task<IActionResult> GetEventFavoriteStatus(int eventId)
    {
        var userId = GetUserId();
        var result = await _favoriteService.GetEventFavoriteStatusAsync(eventId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Retorna todos os eventos favoritos do usuário logado
    /// </summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetUserFavoriteEvents()
    {
        var userId = GetUserId();
        var result = await _favoriteService.GetUserFavoriteEventsAsync(userId);
        return Ok(result);
    }

    // ==================== SEGUIR ORGANIZADORES ====================

    /// <summary>
    /// Alterna o estado de seguir um organizador (seguir/deixar de seguir)
    /// </summary>
    [HttpPost("organizers/{organizerId}/toggle")]
    public async Task<IActionResult> ToggleOrganizerFollow(int organizerId)
    {
        var userId = GetUserId();
        var result = await _favoriteService.ToggleOrganizerFollowAsync(organizerId, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Verifica se o usuário logado segue um organizador
    /// </summary>
    [HttpGet("organizers/{organizerId}/status")]
    public async Task<IActionResult> GetFollowStatus(int organizerId)
    {
        var userId = GetUserId();
        var result = await _favoriteService.GetFollowStatusAsync(organizerId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Retorna todos os organizadores que o usuário logado segue
    /// </summary>
    [HttpGet("organizers")]
    public async Task<IActionResult> GetUserFollowedOrganizers()
    {
        var userId = GetUserId();
        var result = await _favoriteService.GetUserFollowedOrganizersAsync(userId);
        return Ok(result);
    }
}
