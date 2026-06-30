'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Users, ArrowLeft, UserMinus, UserPlus, Calendar, MapPin } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { useAuth } from '@/hooks/use-auth';
import { useFavorite } from '@/hooks/use-favorite';
import { toast } from 'sonner';

const API_BASE_URL = 'http://localhost:5188';

interface FollowedOrganizer {
  organizerId: number;
  organizerName: string;
  organizerAvatar: string | null;
  followersCount: number;
  followedAt: string;
}

export default function SeguindoPage() {
  const { isAuthenticated, isLoading: authLoading, getAuthHeaders } = useAuth();
  const { toggleOrganizerFollow, loadingOrgId } = useFavorite();
  const [organizers, setOrganizers] = useState<FollowedOrganizer[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (authLoading) return;
    if (!isAuthenticated) {
      setIsLoading(false);
      return;
    }

    async function fetchFollowed() {
      try {
        const headers = getAuthHeaders();
        const res = await fetch(`${API_BASE_URL}/api/favorites/organizers`, {
          headers: { 'Authorization': headers['Authorization'] || '' },
        });
        const data = await res.json();
        if (data.success) {
          setOrganizers(data.data || []);
        }
      } catch (err) {
        console.error('Erro ao carregar seguidos:', err);
      } finally {
        setIsLoading(false);
      }
    }

    fetchFollowed();
  }, [authLoading, isAuthenticated, getAuthHeaders]);

  const handleUnfollow = async (organizerId: number) => {
    const result = await toggleOrganizerFollow(organizerId);
    if (result && !result.isFollowing) {
      setOrganizers(prev => prev.filter(o => o.organizerId !== organizerId));
    }
  };

  if (authLoading || isLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="mb-4 h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto" />
            <p className="text-muted-foreground">Carregando...</p>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <Users className="mx-auto mb-4 h-16 w-16 text-muted-foreground/50" />
            <h1 className="mb-2 text-2xl font-bold">Organizadores Seguidos</h1>
            <p className="mb-6 text-muted-foreground">Faça login para ver os organizadores que você segue.</p>
            <Link href="/login">
              <Button className="bg-primary text-primary-foreground">Fazer Login</Button>
            </Link>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="flex-1">
        <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8 flex items-center gap-4">
            <Link href="/">
              <Button variant="ghost" size="icon">
                <ArrowLeft className="h-5 w-5" />
              </Button>
            </Link>
            <div>
              <h1 className="text-2xl font-bold text-foreground sm:text-3xl">
                Organizadores Seguidos
              </h1>
              <p className="text-muted-foreground">
                {organizers.length} {organizers.length === 1 ? 'organizador seguido' : 'organizadores seguidos'}
              </p>
            </div>
          </div>

          {organizers.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <Users className="mb-4 h-16 w-16 text-muted-foreground/30" />
              <h2 className="mb-2 text-xl font-semibold">Nenhum organizador seguido</h2>
              <p className="mb-6 max-w-md text-muted-foreground">
                Ao seguir um organizador, você verá os eventos dele aqui.
                Vá até a página de um evento e clique em "Seguir".
              </p>
              <Link href="/">
                <Button className="bg-primary text-primary-foreground">
                  Explorar Eventos
                </Button>
              </Link>
            </div>
          ) : (
            <div className="space-y-3">
              {organizers.map((org) => (
                <Card key={org.organizerId} className="transition-all hover:shadow-md">
                  <CardContent className="flex items-center justify-between p-4 sm:p-6">
                    <div className="flex items-center gap-4">
                      <Avatar className="h-14 w-14">
                        <AvatarImage src={org.organizerAvatar || ''} alt={org.organizerName} />
                        <AvatarFallback className="bg-primary/10 text-lg font-semibold text-primary">
                          {org.organizerName.charAt(0).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <div>
                        <h3 className="text-lg font-semibold text-foreground">
                          {org.organizerName}
                        </h3>
                        <div className="flex items-center gap-3 text-sm text-muted-foreground">
                          <span className="flex items-center gap-1">
                            <Users className="h-3.5 w-3.5" />
                            {org.followersCount.toLocaleString('pt-BR')} seguidores
                          </span>
                          <span>
                            Seguindo desde {new Date(org.followedAt).toLocaleDateString('pt-BR')}
                          </span>
                        </div>
                      </div>
                    </div>

                    <Button
                      variant="outline"
                      size="sm"
                      className="gap-2 text-red-500 hover:bg-red-50 hover:text-red-600 border-red-200 shrink-0"
                      onClick={() => handleUnfollow(org.organizerId)}
                      disabled={loadingOrgId === org.organizerId}
                    >
                      <UserMinus className="h-4 w-4" />
                      <span className="hidden sm:inline">Deixar de Seguir</span>
                    </Button>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      </main>

      <Footer />
    </div>
  );
}
