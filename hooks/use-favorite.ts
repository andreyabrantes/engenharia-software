'use client';

import { useState, useCallback } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { toast } from 'sonner';

const API_BASE_URL = 'http://localhost:5188';

interface FavoriteStatus {
  isFavorited: boolean;
  eventId?: number;
}

interface FollowStatus {
  isFollowing: boolean;
  followersCount: number;
  organizerId?: number;
}

/**
 * Hook para gerenciar favoritos de eventos e seguir organizadores
 */
export function useFavorite() {
  const { isAuthenticated, getAuthHeaders } = useAuth();
  const [loadingEventId, setLoadingEventId] = useState<number | null>(null);
  const [loadingOrgId, setLoadingOrgId] = useState<number | null>(null);

  /**
   * Alterna favorito de um evento
   */
  const toggleEventFavorite = useCallback(async (eventId: number): Promise<boolean | null> => {
    if (!isAuthenticated) {
      toast.error('Faça login para favoritar eventos');
      return null;
    }

    setLoadingEventId(eventId);
    try {
      const headers = getAuthHeaders();
      // Remove Content-Type for POST with no body
      const res = await fetch(`${API_BASE_URL}/api/favorites/events/${eventId}/toggle`, {
        method: 'POST',
        headers: {
          'Authorization': headers['Authorization'] || '',
          'Content-Type': 'application/json',
        },
      });

      const data = await res.json();
      if (data.success) {
        toast.success(data.data.message);
        return data.data.isFavorited;
      } else {
        toast.error(data.message || 'Erro ao favoritar evento');
        return null;
      }
    } catch (err) {
      toast.error('Erro de conexão ao favoritar evento');
      return null;
    } finally {
      setLoadingEventId(null);
    }
  }, [isAuthenticated, getAuthHeaders]);

  /**
   * Verifica status de favorito de um evento
   */
  const getEventFavoriteStatus = useCallback(async (eventId: number): Promise<FavoriteStatus | null> => {
    if (!isAuthenticated) return null;

    try {
      const headers = getAuthHeaders();
      const res = await fetch(`${API_BASE_URL}/api/favorites/events/${eventId}/status`, {
        headers: {
          'Authorization': headers['Authorization'] || '',
        },
      });

      const data = await res.json();
      if (data.success) {
        return data.data;
      }
      return null;
    } catch {
      return null;
    }
  }, [isAuthenticated, getAuthHeaders]);

  /**
   * Alterna seguir um organizador
   */
  const toggleOrganizerFollow = useCallback(async (organizerId: number): Promise<FollowStatus | null> => {
    if (!isAuthenticated) {
      toast.error('Faça login para seguir organizadores');
      return null;
    }

    setLoadingOrgId(organizerId);
    try {
      const headers = getAuthHeaders();
      const res = await fetch(`${API_BASE_URL}/api/favorites/organizers/${organizerId}/toggle`, {
        method: 'POST',
        headers: {
          'Authorization': headers['Authorization'] || '',
          'Content-Type': 'application/json',
        },
      });

      const data = await res.json();
      if (data.success) {
        toast.success(data.data.message);
        return data.data;
      } else {
        toast.error(data.message || 'Erro ao seguir organizador');
        return null;
      }
    } catch (err) {
      toast.error('Erro de conexão ao seguir organizador');
      return null;
    } finally {
      setLoadingOrgId(null);
    }
  }, [isAuthenticated, getAuthHeaders]);

  /**
   * Verifica status de seguir um organizador
   */
  const getFollowStatus = useCallback(async (organizerId: number): Promise<FollowStatus | null> => {
    if (!isAuthenticated) return null;

    try {
      const headers = getAuthHeaders();
      const res = await fetch(`${API_BASE_URL}/api/favorites/organizers/${organizerId}/status`, {
        headers: {
          'Authorization': headers['Authorization'] || '',
        },
      });

      const data = await res.json();
      if (data.success) {
        return data.data;
      }
      return null;
    } catch {
      return null;
    }
  }, [isAuthenticated, getAuthHeaders]);

  return {
    toggleEventFavorite,
    getEventFavoriteStatus,
    toggleOrganizerFollow,
    getFollowStatus,
    loadingEventId,
    loadingOrgId,
  };
}
