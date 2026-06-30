using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using BoraAli.Api.DTOs;
using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BoraAli.Api.Services;

/// <summary>
/// Serviço responsável por autenticação e autorização
/// </summary>
public class AuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterUserDto registerDto)
    {
        try
        {
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == registerDto.Email);
            if (existingUsers.Any())
                return ApiResponseDto<AuthResponseDto>.Fail("Este email já está cadastrado");

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var created = await _unitOfWork.Users.AddAsync(user);

            var token = GenerateJwtToken(created);
            _logger.LogInformation("Novo usuário registrado: {Email}", registerDto.Email);

            return ApiResponseDto<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(created),
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            }, "Conta criada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário");
            return ApiResponseDto<AuthResponseDto>.Fail("Erro ao registrar usuário", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == loginDto.Email);
            var user = users.FirstOrDefault();

            if (user == null)
                return ApiResponseDto<AuthResponseDto>.Fail("Usuário não encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return ApiResponseDto<AuthResponseDto>.Fail("Senha incorreta. Tente novamente.");

            if (!user.IsActive)
                return ApiResponseDto<AuthResponseDto>.Fail("Conta desativada. Entre em contato com o suporte.");

            var token = GenerateJwtToken(user);
            _logger.LogInformation("Usuário logado: {Email}", loginDto.Email);

            return ApiResponseDto<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            }, "Login realizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login");
            return ApiResponseDto<AuthResponseDto>.Fail("Erro ao fazer login", new() { ex.Message });
        }
    }

    public async Task<ApiResponseDto<UserDto>> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ApiResponseDto<UserDto>.Fail("Usuário não encontrado");
            return ApiResponseDto<UserDto>.Ok(_mapper.Map<UserDto>(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário {UserId}", userId);
            return ApiResponseDto<UserDto>.Fail("Erro ao buscar usuário", new() { ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey não configurada. Defina a variável de ambiente 'JwtSettings__SecretKey' " +
                "ou utilize 'dotnet user-secrets set \"JwtSettings:SecretKey\" \"<chave-aqui>\"'.");
        var issuer = jwtSettings["Issuer"] ?? "BoraAli";
        var audience = jwtSettings["Audience"] ?? "BoraAliUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
