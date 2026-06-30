'use client';

import { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Calendar, MapPin, Clock, Users, Share2, Heart, ArrowLeft, ExternalLink, HeartOff } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { TicketSelector } from '@/components/ticket-selector';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import { ApiEvent, ApiResponse, mapApiEventToLegacy } from '@/lib/api-types';
import { Event } from '@/lib/mock-data';
import { useAuth } from '@/hooks/use-auth';
import { useFavorite } from '@/hooks/use-favorite';
import { getGoogleMapsUrl } from '@/lib/utils';
import { toast } from 'sonner';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5188';

interface EventPageProps {
  params: Promise<{ id: string }>;
}

export default function EventPage({ params }: EventPageProps) {
  const router = useRouter();
  const [event, setEvent] = useState<Event | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
    const [eventId, setEventId] = useState<string>('');
  const [isFavorited, setIsFavorited] = useState(false);
  const [isFollowing, setIsFollowing] = useState(false);
  const [followersCount, setFollowersCount] = useState(0);
  const { isAuthenticated } = useAuth();
  const { toggleEventFavorite, getEventFavoriteStatus, toggleOrganizerFollow, getFollowStatus, loadingEventId, loadingOrgId } = useFavorite();

  // Resolve params
  useEffect(() => {
    params.then((resolved) => setEventId(resolved.id));
  }, [params]);

  // Busca evento da API
  useEffect(() => {
    if (!eventId) return;

    async function fetchEvent() {
      try {
        const res = await fetch(`${API_BASE_URL}/api/events/${eventId}`);
        const data: ApiResponse<ApiEvent> = await res.json();

        if (data.success) {
          setEvent(mapApiEventToLegacy(data.data));
        } else {
          setError('Evento não encontrado');
        }
      } catch (err) {
        console.error('Erro ao carregar evento:', err);
        setError('Erro ao carregar evento');
      } finally {
        setIsLoading(false);
      }
    }

        fetchEvent();
  }, [eventId]);

    // Verifica status de favorito e seguir quando o evento carrega
  useEffect(() => {
    if (!event || !isAuthenticated) return;
    
    const eventNumberId = parseInt(event.id);
    getEventFavoriteStatus(eventNumberId).then(status => {
      if (status) setIsFavorited(status.isFavorited);
    });

    const organizerId = (event.organizer as any).id;
    if (organizerId && organizerId > 0) {
      getFollowStatus(organizerId).then(status => {
        if (status) {
          setIsFollowing(status.isFollowing);
          setFollowersCount(status.followersCount);
        }
      });
    }
  }, [event, isAuthenticated, getEventFavoriteStatus, getFollowStatus]);

  const handleToggleFavorite = async () => {
    if (!event) return;
    const eventNumberId = parseInt(event.id);
    const result = await toggleEventFavorite(eventNumberId);
    if (result !== null) {
      setIsFavorited(result);
    }
  };

    const handleToggleFollow = async () => {
    if (!event) return;
    const organizerId = (event.organizer as any).id;
    if (!organizerId || organizerId === 0) {
      toast.error('Não é possível seguir este organizador');
      return;
    }
    
    const result = await toggleOrganizerFollow(organizerId);
    if (result) {
      setIsFollowing(result.isFollowing);
      setFollowersCount(result.followersCount);
    }
  };

    const handleProceedToCheckout = (selectedTickets: { [key: string]: number }) => {
    if (!event) return;
    sessionStorage.setItem('selectedTickets', JSON.stringify(selectedTickets));
    sessionStorage.setItem('eventId', event.id);
    router.push(`/checkout/${event.id}`);
  };

  const handleShare = async () => {
    const url = window.location.href;
    try {
      await navigator.clipboard.writeText(url);
      toast.success('Link copiado para a área de transferência!', { duration: 2000 });
    } catch {
      toast.error('Erro ao copiar link');
    }
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="mb-4 h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto" />
            <p className="text-muted-foreground">Carregando evento...</p>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (error || !event) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <h1 className="mb-4 text-2xl font-bold">Evento não encontrado</h1>
            <p className="mb-6 text-muted-foreground">
              O evento que você está procurando não existe ou foi removido.
            </p>
            <Link href="/">
              <Button className="bg-primary text-primary-foreground">
                Voltar para a página inicial
              </Button>
            </Link>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  const categoryName = event.category;

  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="flex-1">
        {/* Hero Banner */}
        <div className="relative h-64 w-full sm:h-80 lg:h-96">
          <Image
            src={event.image}
            alt={event.title}
            fill
            className="object-cover"
            priority
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/30 to-transparent" />
          
          {/* Back Button */}
          <div className="absolute left-4 top-4 sm:left-6">
            <Button
              variant="secondary"
              size="sm"
              className="gap-2 bg-white/90 hover:bg-white"
              onClick={() => router.back()}
            >
              <ArrowLeft className="h-4 w-4" />
              Voltar
            </Button>
          </div>

          {/* Category Badge */}
          <div className="absolute bottom-4 left-4 sm:bottom-6 sm:left-6">
            <Badge className="bg-primary text-primary-foreground">
              {categoryName}
            </Badge>
          </div>
        </div>

        {/* Content */}
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="grid gap-8 lg:grid-cols-3">
            {/* Left Column - Event Details */}
            <div className="lg:col-span-2">
              {/* Title and Actions */}
              <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
                <h1 className="text-2xl font-bold text-foreground sm:text-3xl lg:text-4xl">
                  {event.title}
                </h1>
                                <div className="flex gap-2">
                  <Button 
                    variant="outline" 
                    size="icon"
                    onClick={handleToggleFavorite}
                    disabled={loadingEventId === parseInt(event.id)}
                    className={isFavorited ? 'text-red-500 hover:text-red-600 border-red-200 hover:border-red-300' : ''}
                  >
                    {isFavorited ? (
                      <Heart className="h-5 w-5 fill-current" />
                    ) : (
                      <Heart className="h-5 w-5" />
                    )}
                  </Button>
                                    <Button variant="outline" size="icon" onClick={handleShare}>
                    <Share2 className="h-5 w-5" />
                  </Button>
                </div>
              </div>

              {/* Quick Info */}
              <div className="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                <div className="flex items-center gap-3 rounded-lg bg-secondary/50 p-4">
                  <Calendar className="h-6 w-6 text-primary" />
                  <div>
                    <p className="text-sm text-muted-foreground">Data</p>
                    <p className="font-medium">{event.date}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3 rounded-lg bg-secondary/50 p-4">
                  <Clock className="h-6 w-6 text-primary" />
                  <div>
                    <p className="text-sm text-muted-foreground">Horário</p>
                    <p className="font-medium">{event.time}</p>
                  </div>
                </div>
                <a
                  href={getGoogleMapsUrl(event)}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-3 rounded-lg bg-secondary/50 p-4 transition-colors hover:bg-primary/10 hover:text-primary group/link sm:col-span-2 lg:col-span-1"
                >
                  <MapPin className="h-6 w-6 text-primary shrink-0" />
                  <div className="min-w-0">
                    <p className="text-sm text-muted-foreground group-hover/link:text-primary transition-colors">Local</p>
                    <p className="font-medium truncate group-hover/link:underline">{event.location}</p>
                    <div className="flex items-center gap-1 mt-1">
                      <ExternalLink className="h-3 w-3 text-primary opacity-0 group-hover/link:opacity-100 transition-opacity" />
                      <span className="text-xs text-primary opacity-0 group-hover/link:opacity-100 transition-opacity">Abrir no Maps</span>
                    </div>
                  </div>
                </a>
              </div>

              {/* Description */}
              <div className="mb-8">
                <h2 className="mb-4 text-xl font-semibold">Sobre o evento</h2>
                <div className="prose prose-slate max-w-none">
                  <p className="text-muted-foreground leading-relaxed">
                    {event.description}
                  </p>
                  <div className="mt-4 whitespace-pre-line text-muted-foreground">
                    {event.fullDescription}
                  </div>
                </div>
              </div>

              <Separator className="my-8" />

              {/* Organizer */}
              <div className="mb-8">
                <h2 className="mb-4 text-xl font-semibold">Organizador</h2>
                <div className="flex items-center justify-between rounded-lg border border-border p-4">
                  <div className="flex items-center gap-4">
                    <Avatar className="h-14 w-14">
                      <AvatarImage src={event.organizer.logo} alt={event.organizer.name} />
                      <AvatarFallback>{event.organizer.name[0]}</AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="font-medium text-foreground">{event.organizer.name}</p>
                                            <p className="flex items-center gap-1 text-sm text-muted-foreground">
                        <Users className="h-4 w-4" />
                        {(isFollowing || followersCount > 0 ? followersCount : event.organizer.followers).toLocaleString('pt-BR')} seguidores
                      </p>
                    </div>
                  </div>
                                    <Button 
                    variant={isFollowing ? "default" : "outline"}
                    onClick={handleToggleFollow}
                    disabled={loadingOrgId !== null}
                    className={isFollowing ? 'bg-primary text-primary-foreground' : ''}
                  >
                    {isFollowing ? 'Seguindo' : 'Seguir'}
                  </Button>
                </div>
              </div>

              <Separator className="my-8" />

              {/* Location Map Link */}
              <div className="mb-8">
                <h2 className="mb-4 text-xl font-semibold">Localização</h2>
                <a
                  href={getGoogleMapsUrl(event)}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="group/location block overflow-hidden rounded-lg border border-border transition-all hover:border-primary/50 hover:shadow-md"
                >
                  <div className="aspect-video bg-secondary/50">
                    <div className="flex h-full flex-col items-center justify-center">
                      <MapPin className="mb-2 h-12 w-12 text-muted-foreground/50 transition-colors group-hover/location:text-primary/70" />
                      <p className="font-medium text-foreground transition-colors group-hover/location:text-primary">{event.location}</p>
                      <p className="text-sm text-muted-foreground">{event.address}</p>
                      <p className="text-sm text-muted-foreground">{event.city}</p>
                      <div className="mt-3 flex items-center gap-1.5 rounded-full bg-primary/10 px-3 py-1 opacity-0 transition-all group-hover/location:opacity-100">
                        <ExternalLink className="h-3.5 w-3.5 text-primary" />
                        <span className="text-xs font-medium text-primary">Abrir no Google Maps</span>
                      </div>
                    </div>
                  </div>
                </a>
              </div>
            </div>

            {/* Right Column - Ticket Selector */}
            <div className="lg:col-span-1">
              <TicketSelector event={event} onProceed={handleProceedToCheckout} />
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
