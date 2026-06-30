'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Heart, Calendar, MapPin, Clock, ArrowLeft, Trash2 } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { useAuth } from '@/hooks/use-auth';
import { useFavorite } from '@/hooks/use-favorite';
import { toast } from 'sonner';

const API_BASE_URL = 'http://localhost:5188';

interface FavoriteEvent {
  eventId: number;
  title: string;
  description: string;
  eventDate: string;
  time: string;
  location: string;
  city: string;
  imageUrl: string;
  categoryName: string;
  categorySlug: string;
  organizerName: string;
  status: string;
  favoritedAt: string;
}

export default function FavoritesPage() {
  const { isAuthenticated, isLoading: authLoading, getAuthHeaders } = useAuth();
  const { toggleEventFavorite, loadingEventId } = useFavorite();
  const [favorites, setFavorites] = useState<FavoriteEvent[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (authLoading) return;
    if (!isAuthenticated) {
      setIsLoading(false);
      return;
    }

    async function fetchFavorites() {
      try {
        const headers = getAuthHeaders();
        const res = await fetch(`${API_BASE_URL}/api/favorites/events`, {
          headers: { 'Authorization': headers['Authorization'] || '' },
        });
        const data = await res.json();
        if (data.success) {
          setFavorites(data.data || []);
        }
      } catch (err) {
        console.error('Erro ao carregar favoritos:', err);
      } finally {
        setIsLoading(false);
      }
    }

    fetchFavorites();
  }, [authLoading, isAuthenticated, getAuthHeaders]);

  const handleRemoveFavorite = async (eventId: number) => {
    const result = await toggleEventFavorite(eventId);
    if (result !== null && !result) {
      setFavorites(prev => prev.filter(f => f.eventId !== eventId));
    }
  };

  if (authLoading || isLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="mb-4 h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto" />
            <p className="text-muted-foreground">Carregando favoritos...</p>
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
            <Heart className="mx-auto mb-4 h-16 w-16 text-muted-foreground/50" />
            <h1 className="mb-2 text-2xl font-bold">Meus Favoritos</h1>
            <p className="mb-6 text-muted-foreground">Faça login para ver seus eventos favoritos.</p>
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
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8 flex items-center gap-4">
            <Link href="/">
              <Button variant="ghost" size="icon">
                <ArrowLeft className="h-5 w-5" />
              </Button>
            </Link>
            <div>
              <h1 className="text-2xl font-bold text-foreground sm:text-3xl">
                Meus Favoritos
              </h1>
              <p className="text-muted-foreground">
                {favorites.length} {favorites.length === 1 ? 'evento salvo' : 'eventos salvos'}
              </p>
            </div>
          </div>

          {favorites.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <Heart className="mb-4 h-16 w-16 text-muted-foreground/30" />
              <h2 className="mb-2 text-xl font-semibold">Nenhum favorito ainda</h2>
              <p className="mb-6 max-w-md text-muted-foreground">
                Salve seus eventos favoritos clicando no coração e encontre tudo aqui.
              </p>
              <Link href="/">
                <Button className="bg-primary text-primary-foreground">
                  Explorar Eventos
                </Button>
              </Link>
            </div>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {favorites.map((fav) => (
                <Card key={fav.eventId} className="group overflow-hidden transition-all hover:shadow-md">
                  <Link href={`/evento/${fav.eventId}`}>
                    <div className="relative aspect-[16/9] overflow-hidden">
                      <Image
                        src={fav.imageUrl || '/placeholder-event.jpg'}
                        alt={fav.title}
                        fill
                        className="object-cover transition-transform duration-300 group-hover:scale-105"
                      />
                      <Badge className="absolute left-3 top-3 bg-primary text-primary-foreground">
                        {fav.categoryName}
                      </Badge>
                    </div>
                  </Link>

                  <CardContent className="p-4">
                    <Link href={`/evento/${fav.eventId}`}>
                      <h3 className="mb-2 line-clamp-2 font-semibold text-foreground transition-colors group-hover:text-primary">
                        {fav.title}
                      </h3>
                    </Link>

                    <div className="mb-3 space-y-1.5 text-sm text-muted-foreground">
                      <div className="flex items-center gap-2">
                        <Calendar className="h-3.5 w-3.5 shrink-0 text-primary" />
                        <span className="line-clamp-1">
                          {new Date(fav.eventDate).toLocaleDateString('pt-BR', {
                            day: '2-digit', month: 'short', year: 'numeric'
                          })} às {fav.time}
                        </span>
                      </div>
                      <div className="flex items-center gap-2">
                        <MapPin className="h-3.5 w-3.5 shrink-0 text-primary" />
                        <span className="line-clamp-1">{fav.location}, {fav.city}</span>
                      </div>
                    </div>

                    <Button
                      variant="outline"
                      size="sm"
                      className="w-full gap-2 text-red-500 hover:bg-red-50 hover:text-red-600 border-red-200"
                      onClick={(e) => {
                        e.preventDefault();
                        handleRemoveFavorite(fav.eventId);
                      }}
                      disabled={loadingEventId === fav.eventId}
                    >
                      <Trash2 className="h-3.5 w-3.5" />
                      Remover dos favoritos
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
