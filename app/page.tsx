'use client';

import { useState, useEffect, useCallback } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { HeroSection } from '@/components/hero-section';
import { EventsGrid } from '@/components/events-grid';
import { ErrorBoundary } from '@/components/error-boundary';
import { ApiEvent, ApiResponse, mapApiEventToLegacy } from '@/lib/api-types';
import { Event } from '@/lib/mock-data';

const API_BASE_URL = 'http://localhost:5188';

export default function HomePage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const categoriaFromUrl = searchParams.get('categoria');

  const [featuredEvents, setFeaturedEvents] = useState<Event[]>([]);
  const [allEvents, setAllEvents] = useState<Event[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  // selectedCategory é sempre o slug vindo da URL (ou 'all')
  const [selectedCategory, setSelectedCategory] = useState<string>(categoriaFromUrl || 'all');

  // Sincroniza selectedCategory com a URL quando navega via header
  useEffect(() => {
    setSelectedCategory(categoriaFromUrl || 'all');
  }, [categoriaFromUrl]);

  // Busca eventos uma única vez ao carregar a página
  useEffect(() => {
    let cancelled = false;
    
    async function fetchAllEvents() {
      setIsLoading(true);
      try {
        const allRes = await fetch(`${API_BASE_URL}/api/events?pageSize=50`);

        if (cancelled) return;

        if (!allRes.ok) return;

        const allData: ApiResponse<{ items: ApiEvent[] }> = await allRes.json();

        if (!cancelled && allData.success && allData.data?.items) {
          const mapped = allData.data.items.map(mapApiEventToLegacy);
          setAllEvents(mapped);
          setFeaturedEvents(mapped.filter(e => e.isFeatured));
        }
      } catch (err) {
        if (!cancelled) {
          console.error('Erro ao carregar eventos da API:', err);
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }
    
    fetchAllEvents();
    return () => { cancelled = true; };
  }, []);

  const handleCategoryChange = useCallback((slug: string) => {
    const params = new URLSearchParams(window.location.search);
    if (slug && slug !== 'all') {
      params.set('categoria', slug);
    } else {
      params.delete('categoria');
    }
    const newUrl = params.toString() ? `/?${params.toString()}` : '/';
    router.replace(newUrl, { scroll: false });
  }, [router]);

    if (isLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="mb-4 h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto" />
            <p className="text-muted-foreground">Carregando eventos...</p>
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
        <ErrorBoundary>
          <HeroSection featuredEvents={featuredEvents} />
        </ErrorBoundary>
        <ErrorBoundary>
          <EventsGrid 
            events={allEvents} 
            title="Todos os Eventos"
            showFilters={true}
            selectedCategory={selectedCategory}
            onCategoryChange={handleCategoryChange}
          />
        </ErrorBoundary>

        <section className="bg-primary py-12 sm:py-16">
          <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
            <h2 className="mb-4 text-2xl font-bold text-primary-foreground sm:text-3xl">
              Não perca nenhum evento!
            </h2>
            <p className="mb-6 text-primary-foreground/80">
              Cadastre-se para receber as melhores ofertas e novidades em primeira mão.
            </p>
            <div className="mx-auto flex max-w-md flex-col gap-3 sm:flex-row">
              <input
                type="email"
                placeholder="Digite seu e-mail"
                className="flex-1 rounded-lg border-0 bg-white/10 px-4 py-3 text-primary-foreground placeholder:text-primary-foreground/60 focus:outline-none focus:ring-2 focus:ring-white/50"
              />
              <button className="rounded-lg bg-white px-6 py-3 font-medium text-primary transition-colors hover:bg-white/90">
                Cadastrar
              </button>
            </div>
          </div>
        </section>
      </main>

      <Footer />
    </div>
  );
}

