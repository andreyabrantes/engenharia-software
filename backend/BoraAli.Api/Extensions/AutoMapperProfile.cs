using AutoMapper;
using BoraAli.Api.DTOs;
using BoraAli.Core.Entities;

namespace BoraAli.Api.Extensions;

/// <summary>
/// Perfil de mapeamento AutoMapper para conversão entre entidades e DTOs
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Event mappings
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.OrganizerName, opt => opt.MapFrom(src => src.Organizer != null ? src.Organizer.Name : null))
            .ForMember(dest => dest.OrganizerAvatar, opt => opt.MapFrom(src => src.Organizer != null ? src.Organizer.AvatarUrl : null))
            .ForMember(dest => dest.OrganizerFollowers, opt => opt.MapFrom(src => src.Organizer != null ? (src.Organizer.Orders != null ? src.Organizer.Orders.Count * 10 : 0) : 0))
            .ForMember(dest => dest.Tickets, opt => opt.MapFrom(src => src.TicketTypes));

        CreateMap<CreateEventDto, Event>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Draft"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.TicketTypes, opt => opt.MapFrom(src => src.Tickets))
            .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => src.Cep))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Street))
            .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Neighborhood))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.AddressNumber, opt => opt.MapFrom(src => src.AddressNumber));

        CreateMap<UpdateEventDto, Event>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // TicketType mappings
        CreateMap<TicketType, TicketTypeDto>();
        CreateMap<CreateTicketTypeDto, TicketType>()
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.TotalQuantity))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.EventCount, opt => opt.MapFrom(src => src.Events != null ? src.Events.Count : 0));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : null))
            .ForMember(dest => dest.EventImageUrl, opt => opt.MapFrom(src => src.Event != null ? src.Event.ImageUrl : null))
            .ForMember(dest => dest.EventDate, opt => opt.MapFrom(src => src.Event != null ? src.Event.EventDate : (DateTime?)null))
            .ForMember(dest => dest.EventLocation, opt => opt.MapFrom(src => src.Event != null ? src.Event.Location : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
            .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => GenerateOrderCode()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.TicketName, opt => opt.MapFrom(src => src.TicketType != null ? src.TicketType.Name : null));

        CreateMap<CreateOrderItemDto, OrderItem>();

        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<RegisterUserDto, User>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Cpf));
    }

    private static string GenerateOrderCode()
    {
        return $"BA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
