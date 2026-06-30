using Microsoft.AspNetCore.Http;

namespace BoraAli.Api.DTOs;

// ===== Event DTOs =====

public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FullDescription { get; set; }
    public DateTime EventDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Cep { get; set; }
    public string? Street { get; set; }
    public string? Neighborhood { get; set; }
    public string? State { get; set; }
    public string? AddressNumber { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int OrganizerId { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerAvatar { get; set; }
    public int OrganizerFollowers { get; set; }
    public List<TicketTypeDto> Tickets { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FullDescription { get; set; }
    public DateTime EventDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Cep { get; set; }
    public string? Street { get; set; }
    public string? Neighborhood { get; set; }
    public string? State { get; set; }
    public string? AddressNumber { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public List<CreateTicketTypeDto> Tickets { get; set; } = new();
}

/// <summary>
/// DTO para receber o formulário multipart com upload de imagem e tickets em JSON
/// </summary>
public class CreateEventFormDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FullDescription { get; set; }
    public DateTime EventDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Cep { get; set; }
    public string? Street { get; set; }
    public string? Neighborhood { get; set; }
    public string? State { get; set; }
    public string? AddressNumber { get; set; }
    public int CategoryId { get; set; }
    public string? TicketsJson { get; set; }
    public IFormFile? Image { get; set; }
}

public class UpdateEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? FullDescription { get; set; }
    public DateTime? EventDate { get; set; }
    public string? Time { get; set; }
    public string? Location { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Cep { get; set; }
    public string? Street { get; set; }
    public string? Neighborhood { get; set; }
    public string? State { get; set; }
    public string? AddressNumber { get; set; }
    public string? ImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
}

// ===== TicketType DTOs =====

public class TicketTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public int TotalQuantity { get; set; }
    public string? Description { get; set; }
}

public class CreateTicketTypeDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalQuantity { get; set; }
    public string? Description { get; set; }
}

// ===== Category DTOs =====

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int EventCount { get; set; }
}

// ===== Order DTOs =====

public class OrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int EventId { get; set; }
    public string? EventTitle { get; set; }
    public string? EventImageUrl { get; set; }
    public DateTime? EventDate { get; set; }
    public string? EventLocation { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class CreateOrderDto
{
    public int EventId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CouponCode { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int TicketTypeId { get; set; }
    public string? TicketName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class CreateOrderItemDto
{
    public int TicketTypeId { get; set; }
    public int Quantity { get; set; }
}

// ===== User DTOs =====

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RegisterUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = "Cliente"; // Cliente ou Organizador
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

// ===== Paged Result DTO =====

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// ===== API Response DTO =====

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponseDto<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponseDto<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

// ===== Coupon DTOs =====

public class CouponValidationDto
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public string? Description { get; set; }
    public bool IsValid { get; set; }
}

public class ValidateCouponRequestDto
{
    public string Code { get; set; } = string.Empty;
    public int EventId { get; set; }
}

public class CheckInRequestDto
{
    public string OrderCode { get; set; } = string.Empty;
}

// ===== Organizer Stats DTO =====

public class OrganizerStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalTicketsAvailable { get; set; }
    public int TotalEvents { get; set; }
    public int TotalOrders { get; set; }
    public List<TicketTypeStatDto> RevenueByTicketType { get; set; } = new();
    public List<EventStatDto> RevenueByEvent { get; set; } = new();
}

public class TicketTypeStatDto
{
    public string Name { get; set; } = string.Empty;
    public int Sold { get; set; }
    public decimal Revenue { get; set; }
}

public class EventStatDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
}

// ===== Publish Event DTO =====

public class PublishEventResponseDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public bool IsPublished { get; set; }
}

// ===== Favorite / Follow DTOs =====

public class FavoriteStatusDto
{
    public bool IsFavorited { get; set; }
    public int? EventId { get; set; }
    public int? UserId { get; set; }
    public string? Message { get; set; }
}

public class FollowStatusDto
{
    public bool IsFollowing { get; set; }
    public int? OrganizerId { get; set; }
    public int FollowersCount { get; set; }
    public string? Message { get; set; }
}

public class FavoriteEventDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? ImageUrl { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public string? OrganizerName { get; set; }
    public bool IsFeatured { get; set; }
    public string? Status { get; set; }
    public DateTime FavoritedAt { get; set; }
}

public class FollowedOrganizerDto
{
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string? OrganizerAvatar { get; set; }
    public int FollowersCount { get; set; }
    public DateTime FollowedAt { get; set; }
}

// ===== Event Sales Summary DTOs =====

public class EventSalesSummaryDto
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalTicketsAvailable { get; set; }
    public double OccupancyRate { get; set; }
    public int TotalOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int PendingOrders { get; set; }
    public List<TicketTypeSalesDto> SalesByTicketType { get; set; } = new();
}

public class TicketTypeSalesDto
{
    public int TicketTypeId { get; set; }
    public string TicketName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Sold { get; set; }
    public int Available { get; set; }
    public decimal Revenue { get; set; }
}
