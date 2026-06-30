// Tipos compatíveis com a API BoraAli (backend)
// Usados para mapear a resposta da API para o formato esperado pelos componentes

const API_BASE_URL = 'http://localhost:5188';

/**
 * Resolve a URL de uma imagem, prefixando a URL base da API quando o caminho é relativo.
 * O backend serve imagens via StaticFiles em wwwroot/uploads/,
 * então caminhos como "/uploads/events/foto.png" precisam do domínio do backend.
 */
export function resolveImageUrl(imageUrl: string | null | undefined): string {
  if (!imageUrl) return '/placeholder-event.jpg';
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;
  if (imageUrl.startsWith('/')) return `${API_BASE_URL}${imageUrl}`;
  return imageUrl;
}

export interface ApiEvent {
  id: number;
  title: string;
  description: string;
  fullDescription?: string;
  eventDate: string;
  time: string;
  location: string;
  address: string;
  city: string;
  cep?: string;
  street?: string;
  neighborhood?: string;
  state?: string;
  addressNumber?: string;
  imageUrl?: string;
  isFeatured: boolean;
  status: string;
  categoryId: number;
  categoryName?: string;
  organizerId: number;
  organizerName?: string;
  organizerAvatar?: string;
  organizerFollowers: number;
  tickets: ApiTicketType[];
  createdAt: string;
}

export interface ApiTicketType {
  id: number;
  name: string;
  price: number;
  availableQuantity: number;
  totalQuantity: number;
  description?: string;
}

export interface ApiCategory {
  id: number;
  name: string;
  slug: string;
  icon?: string;
  eventCount: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Mapeia um evento da API para o formato usado pelos componentes (legado mock)
export function mapApiEventToLegacy(apiEvent: ApiEvent) {
  const categorySlug = getCategorySlug(apiEvent.categoryName || '');
  return {
    id: String(apiEvent.id),
    title: apiEvent.title,
    description: apiEvent.description,
    fullDescription: apiEvent.fullDescription || '',
    date: formatDate(apiEvent.eventDate),
    time: apiEvent.time,
    location: apiEvent.location,
    address: apiEvent.address,
    city: apiEvent.city,
    category: categorySlug,
    image: resolveImageUrl(apiEvent.imageUrl),
        organizer: {
      id: apiEvent.organizerId,
      name: apiEvent.organizerName || 'Organizador',
      logo: apiEvent.organizerAvatar || '',
      followers: apiEvent.organizerFollowers,
    },
    tickets: (apiEvent.tickets || []).map((t) => ({
      id: String(t.id),
      name: t.name,
      price: t.price,
      available: t.availableQuantity,
      description: t.description,
    })),
    isFeatured: apiEvent.isFeatured,
  };
}

function formatDate(dateStr: string): string {
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).replace('.', '');
  } catch {
    return dateStr;
  }
}

function getCategorySlug(categoryName: string): string {
  // Mapeamento dos nomes das categorias vindas da API para slugs
  const slugMap: Record<string, string> = {
    'Shows': 'shows',
    'Teatro': 'teatro',
    'Esportes': 'esportes',
    'Festivais': 'festivais',
    'Cursos': 'cursos',
    'Gastronomia': 'gastronomia',
    'Tecnologia': 'tecnologia',
    'Infantil': 'infantil',
    'Festas': 'festas',
    'Tech': 'tech',
    'Workshops': 'workshops',
    'Networking': 'networking',
  };
  return slugMap[categoryName] || categoryName.toLowerCase();
}
