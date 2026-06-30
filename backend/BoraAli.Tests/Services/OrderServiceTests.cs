using System.Data;
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
/// Testes unitários para o OrderService
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<IDapperExecutor> _dapperMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _dapperMock = new Mock<IDapperExecutor>();
        _configurationMock = new Mock<IConfiguration>();
        _emailServiceMock = new Mock<IEmailService>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = config.CreateMapper();

        _orderService = new OrderService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object, _dapperMock.Object, _emailServiceMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ReturnsOrderDto()
    {
        // Arrange
        var userId = 1;
        var createDto = new CreateOrderDto
        {
            EventId = 1,
            PaymentMethod = "CreditCard",
            Items = new List<CreateOrderItemDto>
            {
                new() { TicketTypeId = 1, Quantity = 2 },
                new() { TicketTypeId = 2, Quantity = 1 }
            }
        };

        var eventEntity = new Event
        {
            Id = 1,
            Title = "Show de Rock",
            Status = "Published",
            TicketTypes = new List<TicketType>
            {
                new() { Id = 1, Name = "Pista", Price = 100, AvailableQuantity = 50, TotalQuantity = 100 },
                new() { Id = 2, Name = "VIP", Price = 200, AvailableQuantity = 30, TotalQuantity = 50 }
            }
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetEventWithDetailsAsync(1))
            .ReturnsAsync(eventEntity);

        _unitOfWorkMock
            .Setup(u => u.Orders.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) =>
            {
                o.Id = 1;
                return o;
            });

        _unitOfWorkMock
            .Setup(u => u.OrderItems.AddAsync(It.IsAny<OrderItem>()))
            .ReturnsAsync((OrderItem oi) => oi);

        // Mock do BeginTransactionAsync e CommitTransactionAsync
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        // Mock do DapperExecutor.ExecuteAsync para retornar linhas afetadas > 0
        _dapperMock
            .Setup(d => d.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(1);

        // Mock do Users.GetByIdAsync para o envio de e-mail (QR Code)
        _unitOfWorkMock
            .Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Name = "Teste", Email = "teste@email.com" });

        // Act
        var result = await _orderService.CreateOrderAsync(createDto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Pedido criado com sucesso", result.Message);
        Assert.Equal(400, result.Data.TotalAmount); // (2 * 100) + (1 * 200) = 400
        Assert.Equal(2, result.Data.Items.Count);

        // Verificar que a transação foi iniciada e confirmada
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);

        // Verificar que o SQL UPDATE foi chamado para cada ticket (via Dapper)
        _dapperMock.Verify(d => d.ExecuteAsync(
            It.Is<string>(s => s.Contains("UPDATE TicketTypes")),
            It.IsAny<object>(),
            It.IsAny<IDbTransaction>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateOrderAsync_WithUnpublishedEvent_ReturnsFail()
    {
        // Arrange
        var createDto = new CreateOrderDto
        {
            EventId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new() { TicketTypeId = 1, Quantity = 1 }
            }
        };

        var eventEntity = new Event
        {
            Id = 1,
            Title = "Evento Rascunho",
            Status = "Draft" // Não publicado
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetEventWithDetailsAsync(1))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _orderService.CreateOrderAsync(createDto, 1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Evento não está disponível para compra", result.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientQuantity_ReturnsFail()
    {
        // Arrange
        var createDto = new CreateOrderDto
        {
            EventId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new() { TicketTypeId = 1, Quantity = 3 } // Disponível: 2, solicitado: 3 → insuficiente
            }
        };

        var eventEntity = new Event
        {
            Id = 1,
            Title = "Show de Rock",
            Status = "Published",
            TicketTypes = new List<TicketType>
            {
                new() { Id = 1, Name = "Pista", Price = 100, AvailableQuantity = 2, TotalQuantity = 100 }
            }
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetEventWithDetailsAsync(1))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _orderService.CreateOrderAsync(createDto, 1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("não possui quantidade suficiente", result.Message);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithOwnership_ReturnsOrderDto()
    {
        // Arrange
        var orderId = 1;
        var userId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            EventId = 1,
            OrderCode = "BA-20240101-ABCD1234",
            TotalAmount = 300,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        var orderItems = new List<OrderItem>
        {
            new() { Id = 1, OrderId = orderId, TicketTypeId = 1, Quantity = 2, UnitPrice = 100, Subtotal = 200 },
            new() { Id = 2, OrderId = orderId, TicketTypeId = 2, Quantity = 1, UnitPrice = 100, Subtotal = 100 }
        };

        _unitOfWorkMock
            .Setup(u => u.Orders.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(u => u.OrderItems.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>()))
            .ReturnsAsync(orderItems);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(orderId, result.Data.Id);
        Assert.Equal("BA-20240101-ABCD1234", result.Data.OrderCode);
        Assert.Equal(2, result.Data.Items.Count);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithWrongUser_ReturnsFail()
    {
        // Arrange
        var orderId = 1;
        var wrongUserId = 2;

        var order = new Order
        {
            Id = orderId,
            UserId = 1, // Dono diferente
            EventId = 1,
            OrderCode = "BA-20240101-ABCD1234",
            TotalAmount = 300,
            Status = "Confirmed"
        };

        _unitOfWorkMock
            .Setup(u => u.Orders.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId, wrongUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Você não tem permissão para visualizar este pedido", result.Message);
    }

    [Fact]
    public async Task CancelOrderAsync_WithValidOrder_RestoresQuantities()
    {
        // Arrange
        var orderId = 1;
        var userId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            EventId = 1,
            OrderCode = "BA-20240101-ABCD1234",
            TotalAmount = 300,
            Status = "Confirmed"
        };

        var orderItems = new List<OrderItem>
        {
            new() { Id = 1, OrderId = orderId, TicketTypeId = 1, Quantity = 2, UnitPrice = 100, Subtotal = 200 },
            new() { Id = 2, OrderId = orderId, TicketTypeId = 2, Quantity = 1, UnitPrice = 100, Subtotal = 100 }
        };

        _unitOfWorkMock
            .Setup(u => u.Orders.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(u => u.OrderItems.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>()))
            .ReturnsAsync(orderItems);

        _unitOfWorkMock
            .Setup(u => u.Orders.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        // Mock do BeginTransactionAsync e CommitTransactionAsync
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        // Mock do DapperExecutor.ExecuteAsync para retornar linhas afetadas
        _dapperMock
            .Setup(d => d.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(1);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Pedido cancelado com sucesso", result.Message);

        // Verificar que a transação foi iniciada e confirmada
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);

        // Verificar que o SQL UPDATE foi chamado para restaurar quantidades (via Dapper)
        _dapperMock.Verify(d => d.ExecuteAsync(
            It.Is<string>(s => s.Contains("UPDATE TicketTypes")),
            It.IsAny<object>(),
            It.IsAny<IDbTransaction>()), Times.Exactly(2));

        _unitOfWorkMock.Verify(u => u.Orders.UpdateAsync(It.Is<Order>(o => o.Status == "Cancelled")), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_WithAlreadyCancelledOrder_ReturnsFail()
    {
        // Arrange
        var orderId = 1;
        var userId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = "Cancelled" // Já cancelado
        };

        _unitOfWorkMock
            .Setup(u => u.Orders.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Este pedido já foi cancelado", result.Message);
    }
}
