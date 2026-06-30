using BoraAli.Api.DTOs;
using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoraAli.Api.Services;

/// <summary>
/// Serviço responsável por favoritar eventos e seguir organizadores
/// </summary>
public class FavoriteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FavoriteService> _logger;
    private readonly IDapperExecutor _dapper;

    public FavoriteService(IUnitOfWork unitOfWork, ILogger<FavoriteService> logger, IDapperExecutor dapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _dapper = dapper;
    }

    // ==================== FAVORITAR EVENTOS ====================

    /// <summary>
    /// Alterna o estado de favorito de um evento para o usuário (toggle)
    /// </summary>
    public async Task<ApiResponseDto<FavoriteStatusDto>> ToggleEventFavoriteAsync(int eventId, int userId)
    {
        try
        {
            // Verifica se o evento existe
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                return ApiResponseDto<FavoriteStatusDto>.Fail("Evento não encontrado");

            // Verifica se já é favorito
            var existing = await _unitOfWork.EventFavorites.FindAsync(f => f.UserId == userId && f.EventId == eventId);
            var existingList = existing.ToList();

            if (existingList.Any())
            {
                // Já é favorito → REMOVER
                await _unitOfWork.EventFavorites.DeleteAsync(existingList.First());
                _logger.LogInformation("Evento {EventId} removido dos favoritos do usuário {UserId}", eventId, userId);
                return ApiResponseDto<FavoriteStatusDto>.Ok(new FavoriteStatusDto
                {
                    IsFavorited = false,
                    Message = "Evento removido dos favoritos"
                });
            }
            else
            {
                // Não é favorito → ADICIONAR
                var favorite = new EventFavorite
                {
                    UserId = userId,
                    EventId = eventId
                };
                await _unitOfWork.EventFavorites.AddAsync(favorite);
                _logger.LogInformation("Evento {EventId} adicionado aos favoritos do usuário {UserId}", eventId, userId);
                return ApiResponseDto<FavoriteStatusDto>.Ok(new FavoriteStatusDto
                {
                    IsFavorited = true,
                    Message = "Evento favoritado com sucesso"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alternar favorito do evento {EventId}", eventId);
            return ApiResponseDto<FavoriteStatusDto>.Fail("Erro ao favoritar evento", new() { ex.Message });
        }
    }

    /// <summary>
    /// Verifica se um evento é favorito do usuário
    /// </summary>
    public async Task<ApiResponseDto<FavoriteStatusDto>> GetEventFavoriteStatusAsync(int eventId, int userId)
    {
        try
        {
            var existing = await _unitOfWork.EventFavorites.FindAsync(f => f.UserId == userId && f.EventId == eventId);
            var isFavorited = existing.Any();

            return ApiResponseDto<FavoriteStatusDto>.Ok(new FavoriteStatusDto
            {
                IsFavorited = isFavorited,
                EventId = eventId,
                UserId = userId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar favorito do evento {EventId}", eventId);
            return ApiResponseDto<FavoriteStatusDto>.Fail("Erro ao verificar favorito", new() { ex.Message });
        }
    }

    /// <summary>
    /// Retorna todos os eventos favoritos de um usuário
    /// </summary>
    public async Task<ApiResponseDto<IEnumerable<FavoriteEventDto>>> GetUserFavoriteEventsAsync(int userId)
    {
        try
        {
            string sql = @"
                SELECT e.Id AS EventId, e.Title, e.Description, e.EventDate, e.Time,
                       e.Location, e.City, e.ImageUrl, e.IsFeatured, e.Status,
                       c.Name AS CategoryName, c.Slug AS CategorySlug,
                       u.Name AS OrganizerName,
                       ef.CreatedAt AS FavoritedAt
                FROM EventFavorites ef
                INNER JOIN Events e ON e.Id = ef.EventId
                LEFT JOIN Categories c ON e.CategoryId = c.Id
                LEFT JOIN Users u ON e.OrganizerId = u.Id
                WHERE ef.UserId = @UserId
                ORDER BY ef.CreatedAt DESC";

            var favorites = await _dapper.QueryAsync<FavoriteEventDto>(sql, new { UserId = userId });
            return ApiResponseDto<IEnumerable<FavoriteEventDto>>.Ok(favorites.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar eventos favoritos do usuário {UserId}", userId);
            return ApiResponseDto<IEnumerable<FavoriteEventDto>>.Fail("Erro ao buscar favoritos", new() { ex.Message });
        }
    }

    // ==================== SEGUIR ORGANIZADORES ====================

    /// <summary>
    /// Alterna o estado de seguir um organizador (toggle)
    /// </summary>
    public async Task<ApiResponseDto<FollowStatusDto>> ToggleOrganizerFollowAsync(int organizerId, int followerId)
    {
        try
        {
            // Não pode seguir a si mesmo
            if (organizerId == followerId)
                return ApiResponseDto<FollowStatusDto>.Fail("Você não pode seguir a si mesmo");

            // Verifica se o organizador existe
            var organizer = await _unitOfWork.Users.GetByIdAsync(organizerId);
            if (organizer == null)
                return ApiResponseDto<FollowStatusDto>.Fail("Organizador não encontrado");

            // Verifica se já segue
            var existing = await _unitOfWork.OrganizerFollows.FindAsync(f => f.FollowerId == followerId && f.OrganizerId == organizerId);
            var existingList = existing.ToList();

            if (existingList.Any())
            {
                // Já segue → DEIXAR DE SEGUIR
                await _unitOfWork.OrganizerFollows.DeleteAsync(existingList.First());

                // Atualiza a contagem
                string countSql = "SELECT COUNT(*) FROM OrganizerFollows WHERE OrganizerId = @OrgId";
                var followersCount = await _dapper.QuerySingleOrDefaultAsync<int>(countSql, new { OrgId = organizerId });

                _logger.LogInformation("Usuário {FollowerId} deixou de seguir organizador {OrganizerId}", followerId, organizerId);
                return ApiResponseDto<FollowStatusDto>.Ok(new FollowStatusDto
                {
                    IsFollowing = false,
                    FollowersCount = followersCount,
                    Message = "Você deixou de seguir este organizador"
                });
            }
            else
            {
                // Não segue → SEGUIR
                var follow = new OrganizerFollow
                {
                    FollowerId = followerId,
                    OrganizerId = organizerId
                };
                await _unitOfWork.OrganizerFollows.AddAsync(follow);

                // Atualiza a contagem
                string countSql = "SELECT COUNT(*) FROM OrganizerFollows WHERE OrganizerId = @OrgId";
                var followersCount = await _dapper.QuerySingleOrDefaultAsync<int>(countSql, new { OrgId = organizerId });

                _logger.LogInformation("Usuário {FollowerId} começou a seguir organizador {OrganizerId}", followerId, organizerId);
                return ApiResponseDto<FollowStatusDto>.Ok(new FollowStatusDto
                {
                    IsFollowing = true,
                    FollowersCount = followersCount,
                    Message = "Você agora segue este organizador"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alternar seguir organizador {OrganizerId}", organizerId);
            return ApiResponseDto<FollowStatusDto>.Fail("Erro ao seguir organizador", new() { ex.Message });
        }
    }

    /// <summary>
    /// Verifica se o usuário segue um organizador
    /// </summary>
    public async Task<ApiResponseDto<FollowStatusDto>> GetFollowStatusAsync(int organizerId, int followerId)
    {
        try
        {
            var existing = await _unitOfWork.OrganizerFollows.FindAsync(f => f.FollowerId == followerId && f.OrganizerId == organizerId);
            var isFollowing = existing.Any();

            // Conta total de seguidores
            string countSql = "SELECT COUNT(*) FROM OrganizerFollows WHERE OrganizerId = @OrgId";
            var followersCount = await _dapper.QuerySingleOrDefaultAsync<int>(countSql, new { OrgId = organizerId });

            return ApiResponseDto<FollowStatusDto>.Ok(new FollowStatusDto
            {
                IsFollowing = isFollowing,
                FollowersCount = followersCount,
                OrganizerId = organizerId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status de seguir");
            return ApiResponseDto<FollowStatusDto>.Fail("Erro ao verificar status", new() { ex.Message });
        }
    }

    /// <summary>
    /// Retorna todos os organizadores que o usuário segue
    /// </summary>
    public async Task<ApiResponseDto<IEnumerable<FollowedOrganizerDto>>> GetUserFollowedOrganizersAsync(int userId)
    {
        try
        {
            string sql = @"
                SELECT u.Id AS OrganizerId, u.Name AS OrganizerName, u.AvatarUrl AS OrganizerAvatar,
                       of2.CreatedAt AS FollowedAt,
                       (SELECT COUNT(*) FROM OrganizerFollows WHERE OrganizerId = of2.OrganizerId) AS FollowersCount
                FROM OrganizerFollows of2
                INNER JOIN Users u ON u.Id = of2.OrganizerId
                WHERE of2.FollowerId = @UserId
                ORDER BY of2.CreatedAt DESC";

            var followed = await _dapper.QueryAsync<FollowedOrganizerDto>(sql, new { UserId = userId });
            return ApiResponseDto<IEnumerable<FollowedOrganizerDto>>.Ok(followed.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar organizadores seguidos pelo usuário {UserId}", userId);
            return ApiResponseDto<IEnumerable<FollowedOrganizerDto>>.Fail("Erro ao buscar seguidos", new() { ex.Message });
        }
    }
}
