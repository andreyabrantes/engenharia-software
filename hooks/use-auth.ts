'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';

const API_BASE_URL = 'http://localhost:5188';

export interface User {
  id: number;
  name: string;
  email: string;
  phone?: string;
  avatarUrl?: string;
  role: string;
  createdAt: string;
}

export function useAuth() {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedToken = localStorage.getItem('@BoraAli:token');
    const storedUser = localStorage.getItem('@BoraAli:user');
    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser));
    }
    setIsLoading(false);
  }, []);

  const login = useCallback((token: string, user: User) => {
    localStorage.setItem('@BoraAli:token', token);
    localStorage.setItem('@BoraAli:user', JSON.stringify(user));
    setToken(token);
    setUser(user);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('@BoraAli:token');
    localStorage.removeItem('@BoraAli:user');
    setToken(null);
    setUser(null);
  }, []);

  const isAuthenticated = !!token;

  const getAuthHeaders = useCallback(() => {
    const headers: Record<string, string> = { 'Content-Type': 'application/json' };
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
  }, [token]);

  return {
    user,
    token,
    isLoading,
    isAuthenticated,
    login,
    logout,
    getAuthHeaders,
    API_BASE_URL,
  };
}

/**
 * Hook para proteger rotas que exigem autenticação.
 * Redireciona para /login se o usuário não estiver autenticado.
 */
export function useRequireAuth() {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isLoading, isAuthenticated, router]);

  return { isAuthenticated, isLoading };
}
