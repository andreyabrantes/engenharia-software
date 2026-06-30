using BoraAli.Core.Entities;

namespace BoraAli.Core.Interfaces;

/// <summary>
/// Interface específica para operações de eventos
/// </summary>
public interface IEventRepository : IGenericRepository<Event>
{
    Task<IEnumerable<Event>> GetFeaturedEventsAsync();
    Task<IEnumerable<Event>> GetEventsByCategoryAsync(int categoryId);
    Task<IEnumerable<Event>> GetEventsByCityAsync(string city);
    Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
    Task<IEnumerable<Event>> GetEventsByOrganizerAsync(int organizerId);
    Task<Event?> GetEventWithDetailsAsync(int id);
}
