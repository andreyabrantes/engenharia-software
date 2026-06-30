'use client';

import { use, useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';
import { CheckoutForm } from '@/components/checkout-form';
import { ApiEvent, ApiResponse, mapApiEventToLegacy } from '@/lib/api-types';
import { Event } from '@/lib/mock-data';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5188';

interface CheckoutPageProps {
  params: Promise<{ id: string }>;
}

export default function CheckoutPage({ params }: CheckoutPageProps) {
  const resolvedParams = use(params);
  const router = useRouter();
  const [selectedTickets, setSelectedTickets] = useState<{ [key: string]: number } | null>(null);
  const [event, setEvent] = useState<Event | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Retrieve selected tickets from sessionStorage
    const storedTickets = sessionStorage.getItem('selectedTickets');
    const storedEventId = sessionStorage.getItem('eventId');

    if (!storedTickets || storedEventId !== resolvedParams.id) {
      // No tickets selected, redirect back to event page
      router.push(`/evento/${resolvedParams.id}`);
      return;
    }

    setSelectedTickets(JSON.parse(storedTickets));
  }, [resolvedParams.id, router]);

  useEffect(() => {
    async function fetchEvent() {
      try {
        const res = await fetch(`${API_BASE_URL}/api/events/${resolvedParams.id}`);
        const data: ApiResponse<ApiEvent> = await res.json();
        if (data.success) {
          setEvent(mapApiEventToLegacy(data.data));
        }
      } catch (err) {
        console.error('Erro ao carregar evento para checkout:', err);
      } finally {
        setIsLoading(false);
      }
    }

    fetchEvent();
  }, [resolvedParams.id]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4"></div>
            <p className="text-muted-foreground">Carregando...</p>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (!event) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <h1 className="mb-4 text-2xl font-bold">Evento não encontrado</h1>
            <p className="mb-6 text-muted-foreground">
              O evento que você está procurando não existe ou foi removido.
            </p>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (!selectedTickets) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex flex-1 items-center justify-center">
          <div className="text-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4"></div>
            <p className="text-muted-foreground">Carregando...</p>
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
        <CheckoutForm event={event} selectedTickets={selectedTickets} />
      </main>
      <Footer />
    </div>
  );
}
