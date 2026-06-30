using System.Linq.Expressions;
using AutoMapper;
using BoraAli.Api.DTOs;
using BoraAli.Api.Extensions;
using BoraAli.Api.Services;
using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoraAli.Tests.Services;

/// <summary>
/// Testes unitários para o AuthService
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Configurar AutoMapper real
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = config.CreateMapper();

        // Configurar IConfiguration em memória
        var configData = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "BoraAli@TestSecretKey2024!@#$%¨&*()_+Test" },
            { "JwtSettings:Issuer", "BoraAliTest" },
            { "JwtSettings:Audience", "BoraAliTestUsers" },
            { "JwtSettings:ExpirationInHours", "8" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _authService = new AuthService(_unitOfWorkMock.Object, _mapper, _configuration, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Name = "Novo Usuário",
            Email = "novo@email.com",
            Password = "senha123",
            Phone = "(11) 99999-0000"
        };

        _unitOfWorkMock
            .Setup(u => u.Users.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>()); // Nenhum usuário existente

        _unitOfWorkMock
            .Setup(u => u.Users.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = 1;
                return u;
            });

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Token);
        Assert.Equal("Novo Usuário", result.Data.User.Name);
        Assert.Equal("novo@email.com", result.Data.User.Email);
        Assert.Equal("Conta criada com sucesso", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFail()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Name = "Usuário Duplicado",
            Email = "existente@email.com",
            Password = "senha123"
        };

        var existingUsers = new List<User>
        {
            new() { Id = 1, Name = "Existente", Email = "existente@email.com" }
        };

        _unitOfWorkMock
            .Setup(u => u.Users.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUsers);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal("Este email já está cadastrado", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "usuario@email.com",
            Password = "senha123"
        };

        var user = new User
        {
            Id = 1,
            Name = "Usuário Teste",
            Email = "usuario@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
            Role = "User",
            IsActive = true
        };

        _unitOfWorkMock
            .Setup(u => u.Users.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Token);
        Assert.Equal("Usuário Teste", result.Data.User.Name);
        Assert.Equal("Login realizado com sucesso", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFail()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "usuario@email.com",
            Password = "senha_errada"
        };

        var user = new User
        {
            Id = 1,
            Name = "Usuário Teste",
            Email = "usuario@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
            IsActive = true
        };

        _unitOfWorkMock
            .Setup(u => u.Users.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal("Senha incorreta. Tente novamente.", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ReturnsFail()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "inativo@email.com",
            Password = "senha123"
        };

        var user = new User
        {
            Id = 1,
            Name = "Usuário Inativo",
            Email = "inativo@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
            IsActive = false
        };

        _unitOfWorkMock
            .Setup(u => u.Users.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Conta desativada. Entre em contato com o suporte.", result.Message);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Name = "Usuário Teste",
            Email = "usuario@email.com",
            Phone = "(11) 99999-0000",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock
            .Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.GetUserByIdAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(userId, result.Data.Id);
        Assert.Equal("Usuário Teste", result.Data.Name);
        Assert.Equal("usuario@email.com", result.Data.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingId_ReturnsFail()
    {
        // Arrange
        var userId = 999;

        _unitOfWorkMock
            .Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.GetUserByIdAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal("Usuário não encontrado", result.Message);
    }
}
