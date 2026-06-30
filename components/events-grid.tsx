'use client';

import { useState, useEffect } from 'react';
import { Event } from '@/lib/mock-data';
import { EventCard } from '@/components/event-card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { ApiCategory } from '@/lib/api-types';

const API_BASE_URL = 'http://localhost:5188';

interface EventsGridProps {
  events: Event[];
  title?: string;
  showFilters?: boolean;
  selectedCategory?: string;
  onCategoryChange?: (category: string) => void;
}

// Mapa de nomes de categorias (fallback)
const CATEGORY_NAMES: Record<string, string> = {
  shows: 'Shows',
  teatro: 'Teatro',
  esportes: 'Esportes',
  festivais: 'Festivais',
  cursos: 'Cursos',
  gastronomia: 'Gastronomia',
  tecnologia: 'Tecnologia',
  infantil: 'Infantil',
};

export function EventsGrid({ events, title = "Eventos em Destaque", showFilters = true, selectedCategory = 'all', onCategoryChange }: EventsGridProps) {
    // Carrega categorias da API uma única vez
  const [apiCategories, setApiCategories] = useState<ApiCategory[]>([]);

  useEffect(() => {
    let cancelled = false;
    fetch(`${API_BASE_URL}/api/events/categories`)
      .then(res => res.json())
      .then(data => {
        if (!cancelled && data.success) {
          setApiCategories(data.data);
        }
      })
      .catch(() => {});
    return () => { cancelled = true; };
  }, []);

    // Filtra eventos localmente
  const filteredEvents = events.filter((event) => {
    return selectedCategory === 'all' || event.category === selectedCategory;
  });

  const handleCategoryChange = (value: string) => {
    if (onCategoryChange) onCategoryChange(value);
  };

  const getCategoryName = (slug: string): string => {
    if (apiCategories.length > 0) {
      const cat = apiCategories.find(c => c.slug === slug);
      if (cat) return cat.name;
    }
    return CATEGORY_NAMES[slug] || slug;
  };

  return (
    <section className="py-8 sm:py-12">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <h2 className="text-2xl font-bold text-foreground sm:text-3xl">{title}</h2>
          
                    {showFilters && (
            <div className="flex flex-wrap gap-3">
              {/* Category Filter */}
              <Select value={selectedCategory} onValueChange={handleCategoryChange}>
                <SelectTrigger className="w-[160px]">
                  <SelectValue placeholder="Categoria" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todas Categorias</SelectItem>
                  {apiCategories.map((category) => (
                    <SelectItem key={category.id} value={category.slug}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}
        </div>

                {/* Active Filters */}
        {selectedCategory !== 'all' && (
          <div className="mb-6 flex flex-wrap items-center gap-2">
            <span className="text-sm text-muted-foreground">Filtro ativo:</span>
            <Badge variant="secondary" className="gap-1">
              {getCategoryName(selectedCategory)}
              <button 
                onClick={() => handleCategoryChange('all')}
                className="ml-1 hover:text-destructive"
              >
                ×
              </button>
            </Badge>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => handleCategoryChange('all')}
              className="text-muted-foreground"
            >
              Limpar filtro
            </Button>
          </div>
        )}

        {/* Events Grid */}
        {filteredEvents.length > 0 ? (
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {filteredEvents.map((event) => (
              <EventCard key={event.id} event={event} />
            ))}
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <p className="mb-2 text-lg font-medium text-foreground">
              Nenhum evento encontrado
            </p>
            <p className="text-muted-foreground">
              Tente ajustar os filtros para encontrar mais eventos.
            </p>
          </div>
        )}

        {/* Load More */}
        {filteredEvents.length >= 8 && (
          <div className="mt-8 flex justify-center">
            <Button variant="outline" size="lg">
              Carregar mais eventos
            </Button>
          </div>
        )}
      </div>
    </section>
  );
}
