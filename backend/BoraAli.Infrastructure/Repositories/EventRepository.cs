using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using BoraAli.Infrastructure.Data;
using Dapper;

namespace BoraAli.Infrastructure.Repositories;

/// <summary>
/// Repositório específico para Eventos com consultas especializadas (SQLite)
/// </summary>
public class EventRepository : GenericRepository<Event>, IEventRepository
{
    public EventRepository(DbSession session) : base(session) { }

    public async Task<IEnumerable<Event>> GetFeaturedEventsAsync()
    {
        var sql = @"
            SELECT e.*, c.*, u.*, t.*
            FROM Events e
            LEFT JOIN Categories c ON e.CategoryId = c.Id
            LEFT JOIN Users u ON e.OrganizerId = u.Id
            LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1
            WHERE e.IsFeatured = 1 AND e.Status = 'Published'
            ORDER BY e.EventDate ASC";

        return await QueryEventsWithRelationsAndTickets(sql);
    }

    public async Task<IEnumerable<Event>> GetEventsByCategoryAsync(int categoryId)
    {
        var sql = @"
            SELECT e.*, c.*, u.*, t.*
            FROM Events e
            LEFT JOIN Categories c ON e.CategoryId = c.Id
            LEFT JOIN Users u ON e.OrganizerId = u.Id
            LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1
            WHERE e.CategoryId = @CategoryId AND e.Status = 'Published'
            ORDER BY e.EventDate ASC";

        return await QueryEventsWithRelationsAndTickets(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<Event>> GetEventsByCityAsync(string city)
    {
        var sql = @"
            SELECT e.*, c.*, u.*, t.*
            FROM Events e
            LEFT JOIN Categories c ON e.CategoryId = c.Id
            LEFT JOIN Users u ON e.OrganizerId = u.Id
            LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1
            WHERE e.City = @City AND e.Status = 'Published'
            ORDER BY e.EventDate ASC";

        return await QueryEventsWithRelationsAndTickets(sql, new { City = city });
    }

    public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
    {
        var sql = @"
            SELECT e.*, c.*, u.*, t.*
            FROM Events e
            LEFT JOIN Categories c ON e.CategoryId = c.Id
            LEFT JOIN Users u ON e.OrganizerId = u.Id
            LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1
            WHERE e.Status = 'Published'
            AND (e.Title LIKE @Search OR e.Description LIKE @Search OR e.City LIKE @Search OR c.Name LIKE @Search)
            ORDER BY e.EventDate ASC";

        return await QueryEventsWithRelationsAndTickets(sql, new { Search = string.Concat("%", searchTerm, "%") });
    }

    public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(int organizerId)
    {
        var sql = @"
            SELECT e.*, c.*, u.*, t.*
            FROM Events e
            LEFT JOIN Categories c ON e.CategoryId = c.Id
            LEFT JOIN Users u ON e.OrganizerId = u.Id
            LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1
            WHERE e.OrganizerId = @OrganizerId
            ORDER BY e.CreatedAt DESC";

        return await QueryEventsWithRelationsAndTickets(sql, new { OrganizerId = organizerId });
    }

    public async Task<Event?> GetEventWithDetailsAsync(int id)
    {
        var sql = @"
            SELECT e.*, c.*, u.*, t.*
            FROM Events e
            LEFT JOIN Categories c ON e.CategoryId = c.Id
            LEFT JOIN Users u ON e.OrganizerId = u.Id
            LEFT JOIN TicketTypes t ON e.Id = t.EventId AND t.IsActive = 1
            WHERE e.Id = @Id";

        var eventDict = new Dictionary<int, Event>();

        await _session.Connection.QueryAsync<Event, Category, User, TicketType, Event>(
            sql,
            (eventEntity, category, user, ticketType) =>
            {
                if (!eventDict.TryGetValue(eventEntity.Id, out var existingEvent))
                {
                    existingEvent = eventEntity;
                    existingEvent.Category = category;
                    existingEvent.Organizer = user;
                    existingEvent.TicketTypes = new List<TicketType>();
                    eventDict.Add(existingEvent.Id, existingEvent);
                }

                if (ticketType != null)
                {
                    existingEvent.TicketTypes.Add(ticketType);
                }

                return existingEvent;
            },
            new { Id = id },
            splitOn: "Id,Id,Id");

        return eventDict.Values.FirstOrDefault();
    }

    /// <summary>
    /// Query com multi-mapping incluindo TicketTypes (4 entidades).
    /// Elimina o problema N+1 das consultas anteriores que faziam JOIN apenas com Category e User.
    /// </summary>
    private async Task<IEnumerable<Event>> QueryEventsWithRelationsAndTickets(string sql, object? parameters = null)
    {
        var eventDict = new Dictionary<int, Event>();

        await _session.Connection.QueryAsync<Event, Category, User, TicketType, Event>(
            sql,
            (eventEntity, category, user, ticketType) =>
            {
                if (!eventDict.TryGetValue(eventEntity.Id, out var existingEvent))
                {
                    existingEvent = eventEntity;
                    existingEvent.Category = category;
                    existingEvent.Organizer = user;
                    existingEvent.TicketTypes = new List<TicketType>();
                    eventDict.Add(existingEvent.Id, existingEvent);
                }

                if (ticketType != null)
                {
                    existingEvent.TicketTypes.Add(ticketType);
                }

                return existingEvent;
            },
            parameters,
            splitOn: "Id,Id,Id");

        return eventDict.Values;
    }
}
