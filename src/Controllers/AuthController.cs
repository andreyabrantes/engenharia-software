using BilheteriaAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilheteriaAPI.Controllers;

public record LoginRequest(string Email, string Senha);

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            Tipo = usuario.Tipo
        });
    }
}
