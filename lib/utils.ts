import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/**
 * Gera um link do Google Maps com o endereço do evento já preenchido.
 * Usa o padrão Universal Link: https://www.google.com/maps/search/?api=1&query=...
 *
 * @param event - Objeto com campos de endereço do evento (todos opcionais)
 * @returns URL completa para abrir o Google Maps com o endereço formatado
 */
export function getGoogleMapsUrl(event: {
  location?: string;
  address?: string;
  city?: string;
  state?: string;
  cep?: string;
  neighborhood?: string;
  street?: string;
  addressNumber?: string;
}): string {
  const parts: string[] = [];

  // Nome do local é o mais importante para o Maps
  if (event.location) parts.push(event.location);

  // Endereço completo: logradouro + número, ou o campo address diretamente
  if (event.street || event.addressNumber) {
    const streetPart = [event.street, event.addressNumber].filter(Boolean).join(', ');
    parts.push(streetPart);
  } else if (event.address) {
    parts.push(event.address);
  }

  if (event.neighborhood) parts.push(event.neighborhood);
  if (event.city) parts.push(event.city);
  if (event.state) parts.push(event.state);
  if (event.cep) parts.push(event.cep);

  const query = encodeURIComponent(parts.join(', '));
  return `https://www.google.com/maps/search/?api=1&query=${query}`;
}
