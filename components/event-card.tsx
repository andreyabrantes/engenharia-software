import { useState, useEffect } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { Calendar, MapPin, ArrowRight, Heart } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardFooter } from '@/components/ui/card';
import { Event, categories } from '@/lib/mock-data';
import { useAuth } from '@/hooks/use-auth';
import { useFavorite } from '@/hooks/use-favorite';
import { getGoogleMapsUrl } from '@/lib/utils';

interface EventCardProps {
  event: Event;
}

export function EventCard({ event }: EventCardProps) {
  const category = categories.find((c) => c.id === event.category);
  const lowestPrice = Math.min(...event.tickets.map((t) => t.price));
  const [isFavorited, setIsFavorited] = useState(false);
  const { isAuthenticated } = useAuth();
  const { toggleEventFavorite, getEventFavoriteStatus, loadingEventId } = useFavorite();

  // Verifica se o evento é favorito ao carregar
  useEffect(() => {
    if (!isAuthenticated) return;
    const eventNumberId = parseInt(event.id);
    if (isNaN(eventNumberId)) return;
    
    getEventFavoriteStatus(eventNumberId).then(status => {
      if (status) setIsFavorited(status.isFavorited);
    });
  }, [event.id, isAuthenticated, getEventFavoriteStatus]);

  const handleToggleFavorite = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    const eventNumberId = parseInt(event.id);
    if (isNaN(eventNumberId)) return;
    
    const result = await toggleEventFavorite(eventNumberId);
    if (result !== null) {
      setIsFavorited(result);
    }
  };

  return (
    <Card className="group overflow-hidden transition-all duration-300 hover:shadow-lg">
      <Link href={`/evento/${event.id}`} className="block">
        {/* Image */}
        <div className="relative aspect-[16/9] overflow-hidden">
          <Image
            src={event.image}
            alt={event.title}
            fill
            priority
            className="object-cover transition-transform duration-300 group-hover:scale-105"
          />
          {/* Category Badge */}
          <Badge 
            className="absolute left-3 top-3 bg-primary text-primary-foreground"
          >
            {category?.name || event.category}
          </Badge>
          
          {/* Favorite Button */}
          {isAuthenticated && (
            <button
              onClick={handleToggleFavorite}
              disabled={loadingEventId === parseInt(event.id)}
              className={`absolute right-3 top-3 flex h-8 w-8 items-center justify-center rounded-full transition-all ${
                isFavorited 
                  ? 'bg-red-500 text-white shadow-md' 
                  : 'bg-black/40 text-white hover:bg-black/60'
              }`}
            >
              <Heart className={`h-4 w-4 ${isFavorited ? 'fill-current' : ''}`} />
            </button>
          )}
        </div>

        {/* Content */}
        <CardContent className="p-4">
          <h3 className="mb-2 line-clamp-2 text-lg font-semibold text-foreground transition-colors group-hover:text-primary">
            {event.title}
          </h3>
          
          <div className="space-y-2 text-sm text-muted-foreground">
            <div className="flex items-center gap-2">
              <Calendar className="h-4 w-4 shrink-0 text-primary" />
              <span>{event.date} às {event.time}</span>
            </div>
            <div className="flex items-center gap-2">
              <MapPin className="h-4 w-4 shrink-0 text-primary" />
              <span
                role="button"
                onClick={(e) => { e.preventDefault(); e.stopPropagation(); window.open(getGoogleMapsUrl(event), '_blank', 'noopener,noreferrer'); }}
                className="line-clamp-1 transition-colors hover:text-primary hover:underline inline-flex items-center gap-1"
                title="Abrir no Google Maps"
              >
                {event.location}, {event.city}
              </span>
            </div>
          </div>
        </CardContent>

        {/* Footer */}
        <CardFooter className="flex items-center justify-between border-t border-border p-4">
          <div>
            <p className="text-xs text-muted-foreground">A partir de</p>
            <p className="text-lg font-bold text-primary">
              {lowestPrice === 0 ? 'Grátis' : `R$ ${lowestPrice.toFixed(2).replace('.', ',')}`}
            </p>
          </div>
          <Button size="sm" className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90">
            Ver Ingressos
            <ArrowRight className="h-4 w-4" />
          </Button>
        </CardFooter>
      </Link>
    </Card>
  );
}
