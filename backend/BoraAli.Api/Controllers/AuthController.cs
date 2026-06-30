using System.Security.Claims;
using BoraAli.Api.DTOs;
using BoraAli.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BoraAli.Api.Controllers;

/// <summary>
/// Controller responsável por autenticação e gerenciamento de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("Login")]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register([FromBody] RegisterUserDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Realiza login do usuário
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("Login")]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    /// <summary>
    /// Obtém dados do usuário autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetMe()
    {
        var userId = GetUserId();
        var result = await _authService.GetUserByIdAsync(userId);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : 0;
    }
}
