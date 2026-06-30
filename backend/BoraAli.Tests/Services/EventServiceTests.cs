using AutoMapper;
using BoraAli.Api.DTOs;
using BoraAli.Api.Extensions;
using BoraAli.Api.Services;
using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace BoraAli.Tests.Services;

/// <summary>
/// Testes unitários para o EventService
/// </summary>
public class EventServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<EventService>> _loggerMock;
    private readonly Mock<IDapperExecutor> _dapperMock;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<EventService>>();
        _dapperMock = new Mock<IDapperExecutor>();

        // Configurar AutoMapper real para os testes
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = config.CreateMapper();

        _eventService = new EventService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object, _dapperMock.Object);
    }

    [Fact]
    public async Task GetEventByIdAsync_WithExistingId_ReturnsEventDto()
    {
        // Arrange
        var eventId = 1;
        var category = new Category { Id = 1, Name = "Shows", Slug = "shows" };
        var organizer = new User { Id = 1, Name = "João", Email = "joao@email.com" };
        var ticketTypes = new List<TicketType>
        {
            new() { Id = 1, EventId = 1, Name = "Pista", Price = 100, TotalQuantity = 100, AvailableQuantity = 80 },
            new() { Id = 2, EventId = 1, Name = "VIP", Price = 200, TotalQuantity = 50, AvailableQuantity = 40 }
        };

        var eventEntity = new Event
        {
            Id = eventId,
            Title = "Show de Rock",
            Description = "Um show incrível",
            EventDate = DateTime.UtcNow.AddDays(30),
            Time = "20:00",
            Location = "Parque",
            Address = "Rua A, 123",
            City = "São Paulo",
            IsFeatured = true,
            Status = "Published",
            CategoryId = 1,
            OrganizerId = 1,
            Category = category,
            Organizer = organizer,
            TicketTypes = ticketTypes
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetEventWithDetailsAsync(eventId))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(eventId, result.Data.Id);
        Assert.Equal("Show de Rock", result.Data.Title);
        Assert.Equal("Shows", result.Data.CategoryName);
        Assert.Equal("João", result.Data.OrganizerName);
        Assert.Equal(2, result.Data.Tickets.Count);
    }

    [Fact]
    public async Task GetEventByIdAsync_WithNonExistingId_ReturnsFail()
    {
        // Arrange
        var eventId = 999;

        _unitOfWorkMock
            .Setup(u => u.Events.GetEventWithDetailsAsync(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        var result = await _eventService.GetEventByIdAsync(eventId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal("Evento não encontrado", result.Message);
    }

    [Fact]
    public async Task CreateEventAsync_WithValidData_ReturnsCreatedEvent()
    {
        // Arrange
        var organizerId = 1;
        var createDto = new CreateEventDto
        {
            Title = "Novo Evento",
            Description = "Descrição do evento",
            EventDate = DateTime.UtcNow.AddDays(60),
            Time = "19:00",
            Location = "Teatro Municipal",
            Address = "Praça Central, s/n",
            City = "Rio de Janeiro",
            CategoryId = 2,
            Tickets = new List<CreateTicketTypeDto>
            {
                new() { Name = "Plateia", Price = 100, TotalQuantity = 200 },
                new() { Name = "Balcão", Price = 60, TotalQuantity = 150 }
            }
        };

        var createdEvent = new Event
        {
            Id = 1,
            Title = createDto.Title,
            Description = createDto.Description,
            EventDate = createDto.EventDate,
            Time = createDto.Time,
            Location = createDto.Location,
            Address = createDto.Address,
            City = createDto.City,
            CategoryId = createDto.CategoryId,
            OrganizerId = organizerId,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock
            .Setup(u => u.Events.AddAsync(It.IsAny<Event>()))
            .ReturnsAsync(createdEvent);

        _unitOfWorkMock
            .Setup(u => u.TicketTypes.AddAsync(It.IsAny<TicketType>()))
            .ReturnsAsync((TicketType t) => t);

        _unitOfWorkMock
            .Setup(u => u.Events.GetEventWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(new Event
            {
                Id = 1,
                Title = createDto.Title,
                Description = createDto.Description,
                EventDate = createDto.EventDate,
                Time = createDto.Time,
                Location = createDto.Location,
                Address = createDto.Address,
                City = createDto.City,
                CategoryId = createDto.CategoryId,
                OrganizerId = organizerId,
                Status = "Draft",
                Category = new Category { Id = 2, Name = "Teatro", Slug = "teatro" },
                Organizer = new User { Id = 1, Name = "João", Email = "joao@email.com" },
                TicketTypes = new List<TicketType>
                {
                    new() { Id = 1, Name = "Plateia", Price = 100, TotalQuantity = 200, AvailableQuantity = 200 },
                    new() { Id = 2, Name = "Balcão", Price = 60, TotalQuantity = 150, AvailableQuantity = 150 }
                }
            });

        // Act
        var result = await _eventService.CreateEventAsync(createDto, organizerId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Novo Evento", result.Data.Title);
        Assert.Equal("Evento criado com sucesso", result.Message);

        // Verificar que AddAsync foi chamado para o evento e para os tickets
        _unitOfWorkMock.Verify(u => u.Events.AddAsync(It.IsAny<Event>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.TicketTypes.AddAsync(It.IsAny<TicketType>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateEventAsync_WithWrongOwner_ReturnsFail()
    {
        // Arrange
        var eventId = 1;
        var wrongUserId = 2;
        var updateDto = new UpdateEventDto { Title = "Título Atualizado" };

        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Evento Original",
            OrganizerId = 1 // Dono diferente
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        // Act
        var result = await _eventService.UpdateEventAsync(eventId, updateDto, wrongUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Você não tem permissão para editar este evento", result.Message);
    }

    [Fact]
    public async Task DeleteEventAsync_WithValidOwner_ReturnsSuccess()
    {
        // Arrange
        var eventId = 1;
        var userId = 1;

        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Evento para Excluir",
            OrganizerId = userId
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        _unitOfWorkMock
            .Setup(u => u.Events.DeleteAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.DeleteEventAsync(eventId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal("Evento excluído com sucesso", result.Message);
        _unitOfWorkMock.Verify(u => u.Events.DeleteAsync(existingEvent), Times.Once);
    }

    [Fact]
    public async Task GetFeaturedEventsAsync_ReturnsFeaturedEvents()
    {
        // Arrange
        var featuredEvents = new List<Event>
        {
            new()
            {
                Id = 1, Title = "Evento Destaque 1", IsFeatured = true, Status = "Published",
                Category = new Category { Id = 1, Name = "Shows", Slug = "shows" },
                Organizer = new User { Id = 1, Name = "Organizador", Email = "org@email.com" },
                TicketTypes = new List<TicketType>()
            },
            new()
            {
                Id = 2, Title = "Evento Destaque 2", IsFeatured = true, Status = "Published",
                Category = new Category { Id = 2, Name = "Teatro", Slug = "teatro" },
                Organizer = new User { Id = 2, Name = "Produtor", Email = "prod@email.com" },
                TicketTypes = new List<TicketType>()
            }
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetFeaturedEventsAsync())
            .ReturnsAsync(featuredEvents);

        // Act
        var result = await _eventService.GetFeaturedEventsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Shows", Slug = "shows", Events = new List<Event> { new(), new() } },
            new() { Id = 2, Name = "Teatro", Slug = "teatro", Events = new List<Event> { new() } },
            new() { Id = 3, Name = "Esportes", Slug = "esportes", Events = new List<Event>() }
        };

        _unitOfWorkMock
            .Setup(u => u.Categories.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _eventService.GetCategoriesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count());

        var categoryList = result.Data.ToList();
        Assert.Equal(2, categoryList[0].EventCount); // Shows tem 2 eventos
        Assert.Equal(1, categoryList[1].EventCount); // Teatro tem 1 evento
        Assert.Equal(0, categoryList[2].EventCount); // Esportes tem 0 eventos
    }

    // ===== Testes para PublishEventAsync =====

    [Fact]
    public async Task PublishEventAsync_ComStatusDraftValido_RetornaEventoPublicado()
    {
        // Arrange
        var eventId = 1;
        var organizerId = 10;
        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Festival de Verão",
            OrganizerId = organizerId,
            Status = "Draft",
            EventDate = DateTime.UtcNow.AddDays(30)
        };
        var activeTickets = new List<TicketType>
        {
            new() { Id = 1, EventId = eventId, Name = "Pista", Price = 150, TotalQuantity = 200, IsActive = true }
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        _unitOfWorkMock
            .Setup(u => u.TicketTypes.FindAsync(It.IsAny<Expression<Func<TicketType, bool>>>()))
            .ReturnsAsync(activeTickets);

        _unitOfWorkMock
            .Setup(u => u.Events.UpdateAsync(It.IsAny<Event>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _eventService.PublishEventAsync(eventId, organizerId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(eventId, result.Data.EventId);
        Assert.Equal("Published", result.Data.Status);
        Assert.True(result.Data.IsPublished);
        Assert.Equal("Festival de Verão", result.Data.Title);
        Assert.Equal("Evento publicado com sucesso! Agora ele está visível para o público.", result.Message);
        _unitOfWorkMock.Verify(u => u.Events.UpdateAsync(It.Is<Event>(e => e.Status == "Published")), Times.Once);
    }

    [Fact]
    public async Task PublishEventAsync_ComStatusDiferenteDeDraft_RetornaFalha()
    {
        // Arrange
        var eventId = 1;
        var organizerId = 10;
        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Evento Já Publicado",
            OrganizerId = organizerId,
            Status = "Published",
            EventDate = DateTime.UtcNow.AddDays(30)
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        // Act
        var result = await _eventService.PublishEventAsync(eventId, organizerId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Contains("Não é possível publicar um evento com status", result.Message);
        Assert.Contains("Published", result.Message);
        _unitOfWorkMock.Verify(u => u.Events.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task PublishEventAsync_ComDataMenosDe24Horas_RetornaFalha()
    {
        // Arrange
        var eventId = 1;
        var organizerId = 10;
        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Evento Em Cima da Hora",
            OrganizerId = organizerId,
            Status = "Draft",
            EventDate = DateTime.UtcNow.AddHours(2)
        };

        _unitOfWorkMock
            .Setup(u => u.Events.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        // Act
        var result = await _eventService.PublishEventAsync(eventId, organizerId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Contains("24 horas", result.Message);
        _unitOfWorkMock.Verify(u => u.Events.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }
}
