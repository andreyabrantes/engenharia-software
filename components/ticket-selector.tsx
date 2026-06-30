'use client';

import { useState } from 'react';
import { Minus, Plus, ShoppingCart } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { Event } from '@/lib/mock-data';

interface TicketSelectorProps {
  event: Event;
  onProceed: (tickets: { [key: string]: number }) => void;
}

export function TicketSelector({ event, onProceed }: TicketSelectorProps) {
  const [selectedTickets, setSelectedTickets] = useState<{ [key: string]: number }>({});

  const updateQuantity = (ticketId: string, delta: number) => {
    setSelectedTickets((prev) => {
      const current = prev[ticketId] || 0;
      const ticket = event.tickets.find((t) => t.id === ticketId);
      const maxAvailable = ticket?.available || 0;
      const newQuantity = Math.max(0, Math.min(current + delta, Math.min(maxAvailable, 10)));
      
      if (newQuantity === 0) {
        const { [ticketId]: _, ...rest } = prev;
        return rest;
      }
      
      return { ...prev, [ticketId]: newQuantity };
    });
  };

  const totalTickets = Object.values(selectedTickets).reduce((sum, qty) => sum + qty, 0);
  const totalPrice = Object.entries(selectedTickets).reduce((sum, [ticketId, qty]) => {
    const ticket = event.tickets.find((t) => t.id === ticketId);
    return sum + (ticket?.price || 0) * qty;
  }, 0);

  const handleProceed = () => {
    if (totalTickets > 0) {
      onProceed(selectedTickets);
    }
  };

  return (
    <Card className="sticky top-24 shadow-lg">
      <CardHeader className="pb-4">
        <CardTitle className="text-lg">Selecione seus ingressos</CardTitle>
        <div className="mt-2 space-y-1 text-sm text-muted-foreground">
          <p className="font-medium text-foreground">{event.date} às {event.time}</p>
          <p>{event.location}</p>
          <p>{event.city}</p>
        </div>
      </CardHeader>

      <Separator />

      <CardContent className="space-y-4 pt-4">
        {event.tickets.map((ticket) => (
          <div 
            key={ticket.id} 
            className="rounded-lg border border-border p-4 transition-colors hover:border-primary/50"
          >
            <div className="mb-3 flex items-start justify-between">
              <div>
                <h4 className="font-medium text-foreground">{ticket.name}</h4>
                {ticket.description && (
                  <p className="text-sm text-muted-foreground">{ticket.description}</p>
                )}
                <p className="mt-1 text-xs text-muted-foreground">
                  {ticket.available > 0 
                    ? `${ticket.available} disponíveis` 
                    : 'Esgotado'}
                </p>
              </div>
              <p className="text-lg font-bold text-primary">
                {ticket.price === 0 
                  ? 'Grátis' 
                  : `R$ ${ticket.price.toFixed(2).replace('.', ',')}`}
              </p>
            </div>

            {/* Quantity Selector */}
            <div className="flex items-center justify-end gap-3">
              <Button
                variant="outline"
                size="icon"
                className="h-8 w-8"
                onClick={() => updateQuantity(ticket.id, -1)}
                disabled={!selectedTickets[ticket.id] || ticket.available === 0}
              >
                <Minus className="h-4 w-4" />
              </Button>
              <span className="w-8 text-center font-medium">
                {selectedTickets[ticket.id] || 0}
              </span>
              <Button
                variant="outline"
                size="icon"
                className="h-8 w-8"
                onClick={() => updateQuantity(ticket.id, 1)}
                disabled={ticket.available === 0 || (selectedTickets[ticket.id] || 0) >= Math.min(ticket.available, 10)}
              >
                <Plus className="h-4 w-4" />
              </Button>
            </div>
          </div>
        ))}
      </CardContent>

      <Separator />

      <CardFooter className="flex-col gap-4 pt-4">
        {/* Summary */}
        {totalTickets > 0 && (
          <div className="w-full space-y-2 rounded-lg bg-secondary/50 p-4">
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Ingressos ({totalTickets})</span>
              <span className="font-medium">R$ {totalPrice.toFixed(2).replace('.', ',')}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Taxa de serviço</span>
              <span className="font-medium">R$ {(totalPrice * 0.1).toFixed(2).replace('.', ',')}</span>
            </div>
            <Separator />
            <div className="flex justify-between">
              <span className="font-medium">Total</span>
              <span className="text-lg font-bold text-primary">
                R$ {(totalPrice * 1.1).toFixed(2).replace('.', ',')}
              </span>
            </div>
          </div>
        )}

        {/* Proceed Button */}
        <Button
          className="w-full gap-2 bg-primary text-primary-foreground hover:bg-primary/90"
          size="lg"
          disabled={totalTickets === 0}
          onClick={handleProceed}
        >
          <ShoppingCart className="h-5 w-5" />
          Continuar para Pagamento
        </Button>
      </CardFooter>
    </Card>
  );
}
